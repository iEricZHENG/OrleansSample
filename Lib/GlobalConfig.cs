using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lib
{
    /// <summary>
    ///  A static class that groups together instances of IConfiguration and allows them to be accessed anywhere in the application
    /// </summary>
    public class GlobalConfig : ConfigurationProvider
    {
        private static Dictionary<string, IConfiguration> Sources { get; } = new Dictionary<string, IConfiguration>();
        public static string ApplicationEnvironment { get; set; }

        /// <summary>
        ///  Add an IConfiguration instance to the AppConfig object
        ///  <param name="configuration">IConfiguration instance</param>
        ///  <param name="sourceName">Name that can be used to refer to this instance</param>
        /// </summary>
        public static void AddConfigurationObject(IConfiguration configuration, string sourceName)
        {
            if (Sources.ContainsKey(sourceName))
            {
                throw new InvalidOperationException($"There is already a configuration source registered with the name {sourceName}");
            }

            Sources.Add(sourceName, configuration);
        }

        /// <summary>
        ///  Remove an IConfiguration instance from the AppConfig object
        ///  <param name="sourceName">Name of the IConfiguration instance to remove</param>
        /// </summary>
        public static void RemoveConfigurationObject(string sourceName)
        {
            if (!Sources.ContainsKey(sourceName))
            {
                throw new InvalidOperationException($"There is no configuration source registered with the name {sourceName}");
            }

            Sources.Remove(sourceName);
        }

        /// <summary>
        ///  Get the value of a specified key from a named IConfiguration instance
        ///  <param name="sourceName">Name of IConfiguration instance which to retrieve the key from</param>
        ///  <param name="key">Name of the key</param>
        /// </summary>
        public static string Get(string sourceName, string key)
        {
            if (!Sources.ContainsKey(sourceName))
            {
                throw new InvalidOperationException($"There is no configuration source registered with the name {sourceName}");
            }

            return Sources[sourceName].GetSection(key).Value;
        }

        /// <summary>
        ///  Set the value of a specified key in a named IConfiguration instance
        ///  <param name="sourceName">Name of IConfiguration instance where the key is to be set</param>
        ///  <param name="key">Name of the key</param>
        ///  <param name="value">Value to assign to the key</param>
        /// </summary>
        public static void Set(string sourceName, string key, string value)
        {
            if (!Sources.ContainsKey(sourceName))
            {
                throw new InvalidOperationException($"There is no configuration source registered with the name {sourceName}");
            }

            Sources[sourceName].GetSection(key).Value = value;
        }

        /// <summary>
        ///  Get all values from the specified configuration instance
        ///  <param name="sourceName">Name of IConfiguration instance where the key is to be set</param>
        ///  <returns>Returns a dictionary containing the configuration key-value pairs</returns>
        /// </summary>
        public static Dictionary<string, string> GetAllValues(string sourceName)
        {
            if (!Sources.ContainsKey(sourceName))
            {
                throw new InvalidOperationException("No configuration source registered with name " + sourceName);
            }

            // Get children of this source and return them
            IConfiguration sourceConfiguration = Sources[sourceName];
            Dictionary<string, string> valuesDictionary = RecurseConfig(sourceConfiguration);

            //Dictionary<string, string> valuesDictionary = sourceConfiguration.GetChildren().ToDictionary(child => child.Key, child => child.Value);
            return valuesDictionary;
        }

        private static Dictionary<string, string> RecurseConfig(IConfiguration source)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            foreach (var child in source.GetChildren())
            {
                if (child.GetChildren().Count() != 0)
                {
                    result = result.Concat(RecurseConfig(child)).GroupBy(d => d.Key).ToDictionary(d => d.Key, d => d.First().Value);
                }

                if (child.GetChildren().Count() != 0 && string.IsNullOrEmpty(child.Value))
                {
                    continue;
                }
                result.Add(child.Path, child.Value);
            }
            return result;
        }
    }
}

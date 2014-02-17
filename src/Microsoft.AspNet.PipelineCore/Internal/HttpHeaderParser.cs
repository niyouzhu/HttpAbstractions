﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNet.PipelineCore.Internal
{
    public static partial class ParsingHelpers
    {
        public static string GetHeader(IDictionary<string, string[]> headers, string key)
        {
            string[] values = GetHeaderUnmodified(headers, key);
            return values == null ? null : string.Join(",", values);
        }

        public static IEnumerable<string> GetHeaderSplit(IDictionary<string, string[]> headers, string key)
        {
            string[] values = GetHeaderUnmodified(headers, key);
            return values == null ? null : GetHeaderSplitImplementation(values);
        }

        public static string[] GetHeaderUnmodified(IDictionary<string, string[]> headers, string key)
        {
            if (headers == null)
            {
                throw new ArgumentNullException("headers");
            }
            string[] values;
            return headers.TryGetValue(key, out values) ? values : null;
        }

        public static void SetHeader(IDictionary<string, string[]> headers, string key, string value)
        {
            if (headers == null)
            {
                throw new ArgumentNullException("headers");
            }
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException("key");
            }
            if (string.IsNullOrWhiteSpace(value))
            {
                headers.Remove(key);
            }
            else
            {
                headers[key] = new[] { value };
            }
        }

        public static void SetHeaderJoined(IDictionary<string, string[]> headers, string key, params string[] values)
        {
            if (headers == null)
            {
                throw new ArgumentNullException("headers");
            }
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException("key");
            }
            if (values == null || values.Length == 0)
            {
                headers.Remove(key);
            }
            else
            {
                headers[key] = new[] { string.Join(",", values.Select(value => QuoteIfNeeded(value))) };
            }
        }

        public static void SetHeaderUnmodified(IDictionary<string, string[]> headers, string key, params string[] values)
        {
            if (headers == null)
            {
                throw new ArgumentNullException("headers");
            }
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException("key");
            }
            if (values == null || values.Length == 0)
            {
                headers.Remove(key);
            }
            else
            {
                headers[key] = values;
            }
        }

        public static void SetHeaderUnmodified(IDictionary<string, string[]> headers, string key, IEnumerable<string> values)
        {
            if (headers == null)
            {
                throw new ArgumentNullException("headers");
            }
            headers[key] = values.ToArray();
        }

        public static void AppendHeader(IDictionary<string, string[]> headers, string key, string values)
        {
            if (string.IsNullOrWhiteSpace(values))
            {
                return;
            }

            string existing = GetHeader(headers, key);
            if (existing == null)
            {
                SetHeader(headers, key, values);
            }
            else
            {
                headers[key] = new[] { existing + "," + values };
            }
        }

        public static void AppendHeaderJoined(IDictionary<string, string[]> headers, string key, params string[] values)
        {
            if (values == null || values.Length == 0)
            {
                return;
            }

            string existing = GetHeader(headers, key);
            if (existing == null)
            {
                SetHeaderJoined(headers, key, values);
            }
            else
            {
                headers[key] = new[] { existing + "," + string.Join(",", values.Select(value => QuoteIfNeeded(value))) };
            }
        }

        public static void AppendHeaderUnmodified(IDictionary<string, string[]> headers, string key, params string[] values)
        {
            if (values == null || values.Length == 0)
            {
                return;
            }

            string[] existing = GetHeaderUnmodified(headers, key);
            if (existing == null)
            {
                SetHeaderUnmodified(headers, key, values);
            }
            else
            {
                SetHeaderUnmodified(headers, key, existing.Concat(values));
            }
        }
        private static IEnumerable<string> GetHeaderSplitImplementation(string[] values)
        {
            foreach (var segment in new HeaderSegmentCollection(values))
            {
                if (segment.Data.HasValue)
                {
                    yield return Unquote(segment.Data.Value);
                }
            }
        }

        // Quote items that contain comas and are not already quoted.
        private static string QuoteIfNeeded(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                // Ignore
            }
            else if (value.Contains(','))
            {
                if (value[0] != '"' || value[value.Length - 1] != '"')
                {
                    value = '"' + value + '"';
                }
            }

            return value;
        }

        private static string Unquote(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                // Ignore
            }
            else if (value.Length > 1 && value[0] == '"' && value[value.Length - 1] == '"')
            {
                value = value.Substring(1, value.Length - 2);
            }

            return value;
        }
    }
}

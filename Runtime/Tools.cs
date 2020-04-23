using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using System;
using UnityEngine.Promise;
using System.Collections.Generic;
using UnityEngine.GameFoundation.Exceptions;

namespace UnityEngine.GameFoundation
{
    internal static class Tools
    {
        /// <summary>
        /// Checks to see if the argument is a valid Id. Valid Ids are alphanumeric with optional dashes or underscores.
        /// No whitespace is permitted
        /// </summary>
        /// <param name="id">id to check</param>
        /// <returns>whether Id is valid or not</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidId(string id)
        {
            return id != null && Regex.IsMatch(id, @"^[\w-_]+$");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
        /// <param name="name"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfArgNull(object o, string name)
        {
            if (o == null)
            {
                throw new ArgumentNullException(name, $"{name} cannot be null");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
        /// <param name="name"></param>
        /// <param name="index"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfArgNull(object o, string name, int index)
        {
            if (o == null)
            {
                throw new ArgumentNullException(name, $"{name}#{index} cannot be null");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="paramName"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ThrowIfOutOfRange
            (int value, int min, int max, string paramName)
        {
            if (value < min || value > max)
            {
                throw new IndexOutOfRangeException
                    ($"{paramName} ({value}) is out of the range [{min}, {max}]");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="name"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfArgNegative(long value, string name)
        {
            if(value < 0)
            {
                throw new ArgumentOutOfRangeException(name, $"Cannot be negative");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="name"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfArgNullOrEmpty(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Cannot be null or empty", name);
            }
        }

        /// <inheritdoc cref="ThrowIfArgNullOrEmpty(string, string)" />
        /// <param name="index"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfArgNullOrEmpty(string value, string name, int index)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Cannot be null or empty", $"{name}#{index}");
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="name"></param>
        /// <param name="rejectable"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool RejectIfArgNullOrEmpty
            (string value, string name, Rejectable rejectable)
        {
            if(string.IsNullOrWhiteSpace(value))
            {
                rejectable.Reject(new ArgumentException("Incorrect value", name));
                return false;
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="name"></param>
        /// <param name="rejectable"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool RejectIfArgNegative
            (long value, string name, Rejectable rejectable)
        {
            if(value < 0)
            {
                var exception =
                    new ArgumentOutOfRangeException(name, "Cannot be negative");

                rejectable.Reject(exception);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Copies a collection into a new array
        /// </summary>
        /// <typeparam name="T">The type of the collection</typeparam>
        /// <param name="collection">The collection of elements to copy into the
        /// new array/</param>
        /// <returns>The new array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] ToArray<T>(ICollection<T> collection)
        {
            var copy = new T[collection.Count];
            collection.CopyTo(copy, 0);
            return copy;
        }

        /// <summary>
        /// Copies a collection into another
        /// </summary>
        /// <typeparam name="T">The type of the collections</typeparam>
        /// <param name="from">The collection of elements to copy</param>
        /// <param name="to">The collection to copy the element into</param>
        /// <returns>The number of item copied</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Copy<T>(ICollection<T> from, ICollection<T> to = null)
        {
            if (to != null)
            {
                to.Clear();
                foreach (var o in from)
                {
                    to.Add(o);
                }
            }

            return from.Count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TElement"></typeparam>
        /// <param name="list"></param>
        public static void Optimize<TElement>(IList<TElement> list)
            where TElement : class
        {
            for(var i = 0; i < list.Count;)
            {
                var element = list[i];
                if(element is null)
                {
                    list.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="statDefinitionId"></param>
        /// <param name="paramName"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StatDefinition GetStatDefinitionOrDie
            (string statDefinitionId, string paramName)
        {
            ThrowIfArgNullOrEmpty(statDefinitionId, paramName);

            var catalog = GameFoundation.catalogs.statCatalog;
            var statDefinition = catalog.FindStatDefinition(statDefinitionId);
            if (statDefinition is null)
            {
                var exception =
                    new ArgumentException(
                        $"{nameof(StatDefinition)} {statDefinitionId} not found",
                        nameof(statDefinitionId));
            }

            return statDefinition;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GameItem GetItemOrDie(string id, string paramName)
        {
            ThrowIfArgNullOrEmpty(id, paramName);

            var item = InventoryManager.FindItem(id);
            if (item is null)
            {
                throw new InventoryItemNotFoundException(id);
            }
            return item;
        }

        /// <summary>
        /// Gets an ivnentiry itme definition from its <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The id of the definition to find.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static InventoryItemDefinition GetInventoryItemDefinitionOrDie
            (string id, string paramName)
        {
            ThrowIfArgNull(id, paramName);

            var catalog = GameFoundation.catalogs.inventoryCatalog;
            var catalogItem = catalog.FindItem(id);
            if (catalogItem is null)
            {
                throw new CatalogItemNotFoundException(id);
            }

            return catalogItem;
        }

        /// <summary>
        /// Checks to see specified namespace exists.
        /// </summary>
        /// <param name="testNamespace">Namespace to check.</param>
        /// <returns>Whether or not specified namespace exists.</returns>
        public static bool NamespaceExists(string testNamespace)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.Namespace == testNamespace)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Checks to see specified type exists.
        /// </summary>
        /// <param name="testType">Type to check.</param>
        /// <returns>Whether or not specified namespace exists.</returns>
        public static bool TypeExists(string testType)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    var typeStr = type.ToString();
                    if (testType == typeStr)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}

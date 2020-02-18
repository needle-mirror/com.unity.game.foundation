using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// A Catalog for all GameItemDefinitions.
    /// </summary>
    public class GameItemCatalog
    {
        /// <summary>
        /// A dictionary of all CategoryDefinitions.
        /// </summary>
        protected internal List<CategoryDefinition> categories { get; }

        /// <summary>
        /// A dictionary of all GameItemDefinitions.
        /// </summary>
        protected internal List<GameItemDefinition> definitions { get; }

        /// <summary>
        /// Constructor to build a GameItemCatalog object.
        /// </summary>
        /// <param name="gameItemDefinitions">The list of GameItemDefinitions that will be available in this catalog for runtime instance instantiation. If null value is passed in an empty list will be created.</param>
        /// <param name="categoryDefinitions">The list of CategoryDefinitions that are the possible categories which could be applied to items in this catalog. If null value is passed in an empty list will be created.</param>
        internal GameItemCatalog(List<GameItemDefinition> gameItemDefinitions, List<CategoryDefinition> categoryDefinitions)
        {
            definitions = gameItemDefinitions ?? new List<GameItemDefinition>();
            categories = categoryDefinitions ?? new List<CategoryDefinition>();
        }

        /// <summary>
        /// Returns specified Category by CategoryDefinition Hash.
        /// </summary>
        /// <param name="categoryId">The Id of the Id of the CategoryDefinition to find.</param>
        /// <returns>The requested CategoryDefinition, or null if an invalid Hash.</returns>
        public CategoryDefinition GetCategory(string categoryId)
        {
            if (string.IsNullOrEmpty(categoryId))
            {
                return null;
            }
            
            return GetCategory(Tools.StringToHash(categoryId));
        }

        /// <summary>
        /// Returns specified CategoryDefinition by its Hash.
        /// </summary>
        /// <param name="categoryHash">The Hash of the CategoryDefinition to find.</param>
        /// <returns>The requested CategoryDefinition, or null if not found.</returns>
        public CategoryDefinition GetCategory(int categoryHash)
        {
            foreach (CategoryDefinition definition in categories)
            {
                if (definition.hash == categoryHash)
                {
                    return definition;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the categories in this catalog in an array.
        /// </summary>
        /// <returns>The categories in this catalog in an array.</returns>
        public CategoryDefinition[] GetCategories()
        {
            return categories?.ToArray();
        }

        /// <summary>
        /// Fills the given list with all categories found in this catalog.
        /// Note: this returns the current state of categories.  To ensure that there
        /// are no invalid or duplicate entries, the 'categories' list will always be 
        /// cleared and 'recycled' (i.e. updated) with current data from the catalog.
        /// </summary>
        /// <param name="categories">The list to clear and fill with all categories.</param>
        public void GetCategories(List<CategoryDefinition> categories)
        {
            if (categories == null)
            {
                return;
            }

            categories.Clear();

            if (this.categories == null)
            {
                return;
            }
            
            categories.AddRange(this.categories);
        }

        /// <summary>
        /// Return specified GameItemDefinition by GameItemDefinition id string.
        /// </summary>
        /// <param name="gameItemDefinitionId">The GameItemDefinition Id string to find.</param>
        /// <returns>Specified GameItemDefinition in this GameItemCatalog.</returns>
        public GameItemDefinition GetGameItemDefinition(string gameItemDefinitionId)
        {
            if (string.IsNullOrEmpty(gameItemDefinitionId))
            {
                return null;
            }
            
            return GetGameItemDefinition(Tools.StringToHash(gameItemDefinitionId));
        }

        /// <summary>
        /// Return specified GameItemDefinition by Hash.
        /// </summary>
        /// <param name="gameItemDefinitionHash">The Hash of the GameItemDefinition to find.</param>
        /// <returns>Specified GameItemDefinition.</returns>
        public GameItemDefinition GetGameItemDefinition(int gameItemDefinitionHash)
        {
            foreach(var definition in definitions)
            {
                if (definition.hash == gameItemDefinitionHash)
                {
                    return definition;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns an array of all game item definitions in this catalog.
        /// </summary>
        /// <returns>An array of all game item definitions in this catalog.</returns>
        public GameItemDefinition[] GetGameItemDefinitions()
        {
            return definitions?.ToArray();
        }

        /// <summary>
        /// Fills the given array with all game item definitions in this catalog.
        /// Note: this returns the current state of game item definitions.  To ensure that there
        /// are no invalid or duplicate entries, the 'gameItemDefinitions' list will always be 
        /// cleared and 'recycled' (i.e. updated) with current data from the catalog.
        /// </summary>
        /// <param name="gameItemDefinitions">The list to clear and fill with all game item definitions.</param>
        public void GetGameItemDefinitions(List<GameItemDefinition> gameItemDefinitions)
        {
            if (gameItemDefinitions == null)
            {
                return;
            }

            gameItemDefinitions.Clear();

            if (definitions == null)
            {
                return;
            }
            
            gameItemDefinitions.AddRange(definitions);
        }

        /// <summary>
        /// This will return an array of GameItemDefinitions with the designated Category.
        /// </summary>
        /// <param name="categoryId">The id string of the Category we want to iterate.</param>
        /// <returns>An array of GameItemDefinitions that contain the given Category.</returns>
        public GameItemDefinition[] GetDefinitionsByCategory(string categoryId)
        {
            if (string.IsNullOrEmpty(categoryId))
            {
                return null;
            }
            
            List<GameItemDefinition> gameItemDefinitions = new List<GameItemDefinition>();
            GetDefinitionsByCategory(categoryId, gameItemDefinitions);

            return gameItemDefinitions.ToArray();
        }

        /// <summary>
        /// Fills the given list with the GameItemDefinitions that have the designated category.
        /// Note: this returns the current state of game item definitions.  To ensure that there
        /// are no invalid or duplicate entries, the 'gameItemDefinitions' list will always be 
        /// cleared and 'recycled' (i.e. updated) with current data from the catalog.
        /// </summary>
        /// <param name="categoryId">The id string of the Category we want to iterate.</param>
        /// <param name="gameItemDefinitions">The list to clear and fill with matching data.</param>
        public void GetDefinitionsByCategory(string categoryId, List<GameItemDefinition> gameItemDefinitions)
        {
            if (gameItemDefinitions == null)
            {
                return;
            }

            gameItemDefinitions.Clear();

            if (string.IsNullOrEmpty(categoryId))
            {
                return;
            }

            GetDefinitionsByCategory(Tools.StringToHash(categoryId), gameItemDefinitions);
        }

        /// <summary>
        /// This will return an array of GameItemDefinitions with the designated Category.
        /// </summary>
        /// <param name="categoryHash">The id hash of the Category we want to iterate.</param>
        /// <returns>An array of GameItemDefinitions that contain the requested Category.</returns>
        public GameItemDefinition[] GetDefinitionsByCategory(int categoryHash)
        {
            if (definitions == null)
            {
                return null;
            }

            List<GameItemDefinition> gameItemDefinitions = new List<GameItemDefinition>();
            GetDefinitionsByCategory(categoryHash, gameItemDefinitions);

            return gameItemDefinitions.ToArray();
        }

        /// <summary>
        /// Fills the given list with the GameItemDefinitions that have the designated category.
        /// Note: this returns the current state of game item definitions.  To ensure that there
        /// are no invalid or duplicate entries, the 'gameItemDefinitions' list will always be 
        /// cleared and 'recycled' (i.e. updated) with current data from the catalog.
        /// </summary>
        /// <param name="categoryHash">The id hash of the Category we want to iterate.</param>
        /// <param name="gameItemDefinitions">The list to clear and fill with matching data.</param>
        public void GetDefinitionsByCategory(int categoryHash, List<GameItemDefinition> gameItemDefinitions)
        {
            if (gameItemDefinitions == null)
            {
                return;
            }

            gameItemDefinitions.Clear();

            if (definitions == null)
            {
                return;
            }

            foreach (var definition in definitions)
            {
                foreach (var category in definition.GetCategories())
                {
                    if (category.hash == categoryHash)
                    {
                        gameItemDefinitions.Add(definition);
                    }
                }
            }
        }

        /// <summary>
        /// Check if the given string's hash is not yet included in this GameItemCatalog's list of GameItemDefinitions and is available for use.
        /// </summary>
        /// <param name="gameItemDefinitionId">The string to be hashed and searched for in this Catalog's list of GameItemDefinition hashes.</param>
        /// <returns>True/False whether or not hash of given string is available for use.</returns>
        public bool IsDefinitionHashUnique(string gameItemDefinitionId)
        {
            if (string.IsNullOrEmpty(gameItemDefinitionId))
            {
                return false;
            }

            return GetGameItemDefinition(Tools.StringToHash(gameItemDefinitionId)) == null;
        }
    }
}

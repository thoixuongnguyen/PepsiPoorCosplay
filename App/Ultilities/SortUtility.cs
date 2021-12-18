
using System.Linq.Dynamic.Core;
using System.Collections;
using System.Reflection;
using System.Text;

namespace AccountManagementV2.App.Ultilities
{
        public class SortUtility<T>
        {

            public static IQueryable<T> SortByMunMeow(IQueryable<T> entity,string orderBy)
        {
            if (!entity.Any())
            {
                return entity;
            }
            if (string.IsNullOrWhiteSpace(orderBy))
            {
                return entity;
            }
            string[] orderParams = orderBy.Trim().Split(' ');
            string orderByProperty = orderParams[0] ;
            string orderByKey = orderParams[1] ;

            switch (orderByKey)
            {
                case "asc":
                        return entity.OrderBy(orderByProperty);

                case "desc":
                    return entity.OrderBy(orderByProperty).Reverse();
            }
            return null;
        }
            public static IQueryable<T> ApplySort(IQueryable<T> entities, string orderByQueryString)
            {
                if (!entities.Any())
                {
                    return entities;
                }

                if (string.IsNullOrWhiteSpace(orderByQueryString))
                {
                    return entities;
                }

                string[] orderParams = orderByQueryString.Trim().Split(',');
                StringBuilder orderQueryBuilder = new();

                foreach (string param in orderParams)
                {
                    if (string.IsNullOrWhiteSpace(param))
                    {
                        continue;
                    }

                    string propertyFromQueryName = param.Trim().Split(" ")[0];
                    PropertyInfo objectProperty = GetPropertyRecursive(typeof(T), propertyFromQueryName);

                    if
                    (
                        objectProperty is null
                        ||
                        (
                            objectProperty is not null &&
                            objectProperty.PropertyType != typeof(string) &&
                            typeof(IEnumerable).IsAssignableFrom(objectProperty.PropertyType)
                        )
                    )
                    {
                        continue;
                    }
                    string sortingOrder = param.EndsWith(" desc") ? "descending" : "ascending";

                    orderQueryBuilder.Append($"{propertyFromQueryName} {sortingOrder}, ");
                }

                string orderQuery = orderQueryBuilder.ToString().TrimEnd(',', ' ');
                if (string.IsNullOrEmpty(orderQuery))
                {
                    return entities;
                }
                return entities.OrderBy(orderQuery);
            }

            private static PropertyInfo GetPropertyRecursive(Type baseType, string propertyName)
            {
                string[] parts = propertyName.Split('.');

                if (baseType.GetProperty(parts[0], BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase) is null)
                {
                    return null;
                }

                return (parts.Length > 1)
                    ? GetPropertyRecursive(baseType.GetProperty(parts[0], BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase).PropertyType, parts.Skip(1).Aggregate((a, i) => a + "." + i))
                    : baseType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            }
        }
}

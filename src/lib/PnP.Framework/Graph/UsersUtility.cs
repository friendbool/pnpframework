﻿using Microsoft.Graph;
using Newtonsoft.Json;
using PnP.Framework.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;

namespace PnP.Framework.Graph
{
    /// <summary>
    /// Provides access to user operations in Microsoft Graph
    /// </summary>
    public static class UsersUtility
    {
        /// <summary>
        /// Returns the user with the provided userId from Azure Active Directory
        /// </summary>
        /// <param name="accessToken">The OAuth 2.0 Access Token to use for invoking the Microsoft Graph</param>
        /// <param name="userId">The unique identifier of the user in Azure Active Directory to return</param>    
        /// <param name="selectProperties">Allows providing the names of properties to return regarding the users. If not provided, the standard properties will be returned.</param>
        /// <param name="startIndex">First item in the results returned by Microsoft Graph to return</param>
        /// <param name="endIndex">Last item in the results returned by Microsoft Graph to return. Provide NULL to return all results that exist.</param>
        /// <param name="retryCount">Number of times to retry the request in case of throttling</param>
        /// <param name="delay">Milliseconds to wait before retrying the request. The delay will be increased (doubled) every retry.</param>
        /// <param name="useBetaEndPoint">Indicates if the v1.0 (false) or beta (true) endpoint should be used at Microsoft Graph to query for the data</param>
        /// <param name="ignoreDefaultProperties">If set to true, only the properties provided through selectProperties will be loaded. The default properties will not be. Optional. Default is that the default properties will always be retrieved.</param>
        /// <returns>List with User objects</returns>
        public static Model.User GetUser(string accessToken, Guid userId, string[] selectProperties = null, int startIndex = 0, int? endIndex = 999, int retryCount = 10, int delay = 500, bool useBetaEndPoint = false, bool ignoreDefaultProperties = false)
        {
            return ListUsers(accessToken, $"id eq '{userId}'", null, selectProperties, startIndex, endIndex, retryCount, delay, ignoreDefaultProperties: ignoreDefaultProperties, useBetaEndPoint: useBetaEndPoint).FirstOrDefault();
        }

        /// <summary>
        /// Returns the user with the provided <paramref name="userPrincipalName"/> from Azure Active Directory
        /// </summary>
        /// <param name="accessToken">The OAuth 2.0 Access Token to use for invoking the Microsoft Graph</param>
        /// <param name="userPrincipalName">The User Principal Name of the user in Azure Active Directory to return</param>
        /// <param name="selectProperties">Allows providing the names of properties to return regarding the users. If not provided, the standard properties will be returned.</param>
        /// <param name="startIndex">First item in the results returned by Microsoft Graph to return</param>
        /// <param name="endIndex">Last item in the results returned by Microsoft Graph to return. Provide NULL to return all results that exist.</param>
        /// <param name="retryCount">Number of times to retry the request in case of throttling</param>
        /// <param name="delay">Milliseconds to wait before retrying the request. The delay will be increased (doubled) every retry.</param>
        /// <param name="useBetaEndPoint">Indicates if the v1.0 (false) or beta (true) endpoint should be used at Microsoft Graph to query for the data</param>
        /// <param name="ignoreDefaultProperties">If set to true, only the properties provided through selectProperties will be loaded. The default properties will not be. Optional. Default is that the default properties will always be retrieved.</param>
        /// <returns>User object</returns>
        public static Model.User GetUser(string accessToken, string userPrincipalName, string[] selectProperties = null, int startIndex = 0, int? endIndex = 999, int retryCount = 10, int delay = 500, bool useBetaEndPoint = false, bool ignoreDefaultProperties = false)
        {
            return ListUsers(accessToken, $"userPrincipalName eq '{userPrincipalName}'", null, selectProperties, startIndex, endIndex, retryCount, delay, ignoreDefaultProperties: ignoreDefaultProperties, useBetaEndPoint: useBetaEndPoint).FirstOrDefault();
        }

        /// <summary>
        /// Returns all the Users in the current domain
        /// </summary>
        /// <param name="accessToken">The OAuth 2.0 Access Token to use for invoking the Microsoft Graph</param> 
        /// <param name="additionalProperties">Allows providing the names of additional properties to return regarding the users</param>
        /// <param name="startIndex">First item in the results returned by Microsoft Graph to return</param>
        /// <param name="endIndex">Last item in the results returned by Microsoft Graph to return. Provide NULL to return all results that exist.</param>
        /// <param name="retryCount">Number of times to retry the request in case of throttling</param>
        /// <param name="delay">Milliseconds to wait before retrying the request. The delay will be increased (doubled) every retry.</param>
        /// <param name="useBetaEndPoint">Indicates if the v1.0 (false) or beta (true) endpoint should be used at Microsoft Graph to query for the data</param>
        /// <param name="ignoreDefaultProperties">If set to true, only the properties provided through selectProperties will be loaded. The default properties will not be. Optional. Default is that the default properties will always be retrieved.</param>
        /// <returns>List with User objects</returns>
        public static List<Model.User> ListUsers(string accessToken, string[] additionalProperties = null, int startIndex = 0, int? endIndex = 999, int retryCount = 10, int delay = 500, bool useBetaEndPoint = false, bool ignoreDefaultProperties = false)
        {
            return ListUsers(accessToken, null, null, additionalProperties, startIndex, endIndex, retryCount, delay, ignoreDefaultProperties: ignoreDefaultProperties, useBetaEndPoint: useBetaEndPoint);
        }

        /// <summary>
        /// Returns all the Users in the current domain filtered out with a custom OData filter
        /// </summary>
        /// <param name="accessToken">The OAuth 2.0 Access Token to use for invoking the Microsoft Graph</param>
        /// <param name="filter">OData filter to apply to retrieval of the users from the Microsoft Graph</param>
        /// <param name="orderby">OData orderby instruction</param>
        /// <param name="selectProperties">Allows providing the names of properties to return regarding the users. If not provided, the standard properties will be returned.</param>
        /// <param name="startIndex">First item in the results returned by Microsoft Graph to return</param>
        /// <param name="endIndex">Last item in the results returned by Microsoft Graph to return. Provide NULL to return all results that exist.</param>
        /// <param name="retryCount">Number of times to retry the request in case of throttling</param>
        /// <param name="delay">Milliseconds to wait before retrying the request. The delay will be increased (doubled) every retry.</param>
        /// <param name="useBetaEndPoint">Indicates if the v1.0 (false) or beta (true) endpoint should be used at Microsoft Graph to query for the data</param>
        /// <param name="ignoreDefaultProperties">If set to true, only the properties provided through selectProperties will be loaded. The default properties will not be. Optional. Default is that the default properties will always be retrieved.</param>
        /// <returns>List with User objects</returns>
        public static List<Model.User> ListUsers(string accessToken, string filter, string orderby, string[] selectProperties = null, int startIndex = 0, int? endIndex = 999, int retryCount = 10, int delay = 500, bool useBetaEndPoint = false, bool ignoreDefaultProperties = false)
        {
            if (String.IsNullOrEmpty(accessToken))
            {
                throw new ArgumentNullException(nameof(accessToken));
            }
            // Rewrite AdditionalProperties to Additional Data
            var propertiesToSelect = ignoreDefaultProperties ? new List<string>() : new List<string> { "BusinessPhones", "DisplayName", "GivenName", "JobTitle", "Mail", "MobilePhone", "OfficeLocation", "PreferredLanguage", "Surname", "UserPrincipalName", "Id", "AccountEnabled" };
            
            selectProperties = selectProperties?.Select(p => p == "AdditionalProperties" ? "AdditionalData" : p).ToArray();
            
            if(selectProperties != null)
            {
                foreach(var property in selectProperties)
                {
                    if(!propertiesToSelect.Contains(property))
                    {
                        propertiesToSelect.Add(property);
                    }
                }
            }

            List<Model.User> result = null;
            try
            {
                // Use a synchronous model to invoke the asynchronous process
                result = Task.Run(async () =>
                {
                    List<Model.User> users = new List<Model.User>();

                    var graphClient = GraphUtility.CreateGraphClient(accessToken, retryCount, delay, useBetaEndPoint: useBetaEndPoint);

                    IGraphServiceUsersCollectionPage pagedUsers;

                    // Retrieve the first batch of users. 999 is the maximum amount of users that Graph allows to be trieved in 1 go. Use maximum size batches to lessen the chance of throttling when retrieving larger amounts of users.
                    pagedUsers = await graphClient.Users.Request()
                                                        .Select(string.Join(",", propertiesToSelect))
                                                        .Filter(filter)
                                                        .OrderBy(orderby)
                                                        .Top(!endIndex.HasValue ? 999 : endIndex.Value >= 999 ? 999 : endIndex.Value)
                                                        .GetAsync();

                    int pageCount = 0;
                    int currentIndex = 0;

                    while (true)
                    {
                        pageCount++;

                        foreach (var pagedUser in pagedUsers)
                        {
                            currentIndex++;

                            if(endIndex.HasValue && endIndex.Value < currentIndex)
                            {
                                break;
                            }

                            if (currentIndex >= startIndex)
                            {
                                users.Add(MapUserEntity(pagedUser, selectProperties));
                            }
                        }

                        if (pagedUsers.NextPageRequest != null && (!endIndex.HasValue || currentIndex < endIndex.Value))
                        {
                            // Retrieve the next batch of users. The possible oData instructions such as select and filter are already incorporated in the nextLink provided by Graph and thus do not need to be specified again.
                            pagedUsers = await pagedUsers.NextPageRequest.GetAsync();
                        }
                        else
                        {
                            break;
                        }
                    }

                    return users;
                }).GetAwaiter().GetResult();
            }
            catch (ServiceException ex)
            {
                Log.Error(Constants.LOGGING_SOURCE, CoreResources.GraphExtensions_ErrorOccured, ex.Error.Message);
                throw;
            }
            return result;
        }

        /// <summary>
        /// Returns the users delta in the current domain filtered out with a custom OData filter. If no <paramref name="deltaToken"/> has been provided, all users will be returned with a deltatoken for a next run. If a <paramref name="deltaToken"/> has been provided, all users which were modified after the deltatoken has been generated will be returned.
        /// </summary>
        /// <param name="accessToken">The OAuth 2.0 Access Token to use for invoking the Microsoft Graph</param>
        /// <param name="deltaToken">DeltaToken to indicate requesting changes since this deltatoken has been created. Leave NULL to retrieve all users with a deltatoken to use for subsequent queries.</param>
        /// <param name="filter">OData filter to apply to retrieval of the users from the Microsoft Graph</param>
        /// <param name="orderby">OData orderby instruction</param>
        /// <param name="selectProperties">Allows providing the names of properties to return regarding the users. If not provided, the standard properties will be returned.</param>
        /// <param name="startIndex">First item in the results returned by Microsoft Graph to return</param>
        /// <param name="endIndex">Last item in the results returned by Microsoft Graph to return. Provide NULL to return all results that exist.</param>
        /// <param name="retryCount">Number of times to retry the request in case of throttling</param>
        /// <param name="delay">Milliseconds to wait before retrying the request. The delay will be increased (doubled) every retry.</param>
        /// <param name="useBetaEndPoint">Indicates if the v1.0 (false) or beta (true) endpoint should be used at Microsoft Graph to query for the data</param>
        /// <param name="ignoreDefaultProperties">If set to true, only the properties provided through selectProperties will be loaded. The default properties will not be. Optional. Default is that the default properties will always be retrieved.</param>
        /// <returns>List with User objects</returns>
        public static Model.UserDelta ListUserDelta(string accessToken, string deltaToken, string filter, string orderby, string[] selectProperties = null, int startIndex = 0, int? endIndex = 999, int retryCount = 10, int delay = 500, bool useBetaEndPoint = false, bool ignoreDefaultProperties = false)
        {
            if (String.IsNullOrEmpty(accessToken))
            {
                throw new ArgumentNullException(nameof(accessToken));
            }
            // Rewrite AdditionalProperties to Additional Data
            var propertiesToSelect = ignoreDefaultProperties ? new List<string>() : new List<string> { "BusinessPhones", "DisplayName", "GivenName", "JobTitle", "Mail", "MobilePhone", "OfficeLocation", "PreferredLanguage", "Surname", "UserPrincipalName", "Id", "AccountEnabled" };
            
            selectProperties = selectProperties?.Select(p => p == "AdditionalProperties" ? "AdditionalData" : p).ToArray();
            
            if(selectProperties != null)
            {
                foreach(var property in selectProperties)
                {
                    if(!propertiesToSelect.Contains(property))
                    {
                        propertiesToSelect.Add(property);
                    }
                }
            }

            var queryOptions = new List<QueryOption>();

            if(!string.IsNullOrWhiteSpace(deltaToken))
            {
                queryOptions.Add(new QueryOption("$skiptoken", deltaToken));
            }

            Model.UserDelta result = null;
            try
            {
                // Use a synchronous model to invoke the asynchronous process
                result = Task.Run(async () =>
                {
                    var usersDelta = new Model.UserDelta();
                    usersDelta.Users = new List<Model.User>();

                    var graphClient = GraphUtility.CreateGraphClient(accessToken, retryCount, delay, useBetaEndPoint: useBetaEndPoint);

                    IUserDeltaCollectionPage pagedUsers;

                    // Retrieve the first batch of users. 999 is the maximum amount of users that Graph allows to be trieved in 1 go. Use maximum size batches to lessen the chance of throttling when retrieving larger amounts of users.
                    pagedUsers = await graphClient.Users.Delta()
                                                        .Request(queryOptions)                    
                                                        .Select(string.Join(",", propertiesToSelect))
                                                        .Filter(filter)
                                                        .OrderBy(orderby)
                                                        .Top(!endIndex.HasValue ? 999 : endIndex.Value >= 999 ? 999 : endIndex.Value)
                                                        .GetAsync();

                    int pageCount = 0;
                    int currentIndex = 0;

                    while (true)
                    {
                        pageCount++;

                        foreach (var pagedUser in pagedUsers)
                        {
                            currentIndex++;

                            if(endIndex.HasValue && endIndex.Value < currentIndex)
                            {
                                break;
                            }

                            if (currentIndex >= startIndex)
                            {
                                usersDelta.Users.Add(MapUserEntity(pagedUser, selectProperties));
                            }
                        }

                        if (pagedUsers.NextPageRequest != null && (!endIndex.HasValue || currentIndex < endIndex.Value))
                        {
                            // Retrieve the next batch of users. The possible oData instructions such as select and filter are already incorporated in the nextLink provided by Graph and thus do not need to be specified again.
                            pagedUsers = await pagedUsers.NextPageRequest.GetAsync();
                        }
                        else
                        {
                            // Check if the deltaLink is provided in the response
                            if(pagedUsers.AdditionalData.TryGetValue("@odata.deltaLink", out object deltaLinkObject))
                            {
                                // Use a regular expression to fetch just the deltatoken part from the deltalink. The base of the URL will thereby be cut off. This is the only part we need to use it in a subsequent run.
                                var deltaLinkMatch = System.Text.RegularExpressions.Regex.Match(deltaLinkObject.ToString(), @"(?<=\$deltatoken=)(.*?)(?=$|&)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                                if(deltaLinkMatch.Success && !string.IsNullOrWhiteSpace(deltaLinkMatch.Value))
                                {
                                    // Successfully extracted the deltatoken part from the link, assign it to the return variable
                                    usersDelta.DeltaToken = deltaLinkMatch.Value;
                                }
                            }
                            break;
                        }
                    }

                    return usersDelta;
                }).GetAwaiter().GetResult();
            }
            catch (ServiceException ex)
            {
                Log.Error(Constants.LOGGING_SOURCE, CoreResources.GraphExtensions_ErrorOccured, ex.Error.Message);
                throw;
            }
            return result;
        }

        /// <summary>
        /// Maps a Graph User result to a local User model
        /// </summary>
        /// <param name="graphUser">Graph User entity</param>
        /// <param name="selectProperties">Properties to copy over from the Graph model to the local User model</param>
        /// <returns>Local User model filled with the information Graph User entity</returns>
        private static Model.User MapUserEntity(User graphUser, string[] selectProperties)
        {
            var user = new Model.User
            {
                Id = Guid.TryParse(graphUser.Id, out Guid idGuid) ? (Guid?)idGuid : null,
                DisplayName = graphUser.DisplayName,
                GivenName = graphUser.GivenName,
                JobTitle = graphUser.JobTitle,
                MobilePhone = graphUser.MobilePhone,
                OfficeLocation = graphUser.OfficeLocation,
                PreferredLanguage = graphUser.PreferredLanguage,
                Surname = graphUser.Surname,
                UserPrincipalName = graphUser.UserPrincipalName,
                BusinessPhones = graphUser.BusinessPhones,
                AdditionalProperties = graphUser.AdditionalData,
                Mail = graphUser.Mail,
                AccountEnabled = graphUser.AccountEnabled,
            };

            // If additional properties have been provided, ensure their output gets added to the AdditionalProperties dictonary of the output
            if (selectProperties != null)
            {
                // Ensure we have the AdditionalProperties dictionary available to fill, if necessary
                if(user.AdditionalProperties == null)
                {
                    user.AdditionalProperties = new Dictionary<
                    string, object>();
                }

                foreach (var selectProperty in selectProperties)
                {
                    // Ensure the requested property has been returned in the response
                    var property = graphUser.GetType().GetProperty(selectProperty, BindingFlags.IgnoreCase |  BindingFlags.Public | BindingFlags.Instance);
                    if (property != null)
                    {
                        // First check if we have the property natively on the User model
                        var userProperty = user.GetType().GetProperty(selectProperty, BindingFlags.IgnoreCase |  BindingFlags.Public | BindingFlags.Instance);
                        if(userProperty != null)
                        {
                            // Set the property on the User model
                            userProperty.SetValue(user, property.GetValue(graphUser), null);
                        }
                        else
                        {
                            // Property does not exist on the User model, add the property to the AdditionalProperties dictionary
                            user.AdditionalProperties.Add(selectProperty, property.GetValue(graphUser));
                        }
                    }
                }
            }

            return user;
        } 
    }
}
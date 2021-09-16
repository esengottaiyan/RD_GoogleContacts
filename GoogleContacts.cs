using Google.Apis.Auth.OAuth2;
using System;
using System.Threading;
using RD_GoogleContacts.Helper;
using Google.GData.Client;
using Google.GData.Contacts;
using Google.GData.Extensions;
using Google.Contacts;
using Google.Apis.Util.Store;
using System.Configuration;
using Newtonsoft.Json;
using NLog;

namespace RD_GoogleContacts
{
    /// <summary>
    /// This is the class which enable to create google contact entries and able to retrieve all the contact entries
    /// </summary>
    public class GoogleContacts
    {
        #region "Variable declaration"
        // Create OAuth credential retrieve from AppSettings.
        private static string clientSecret = ConfigurationManager.AppSettings["clientSecret"];
        private static string clientId = ConfigurationManager.AppSettings["clientId"];
        private static string appName = ConfigurationManager.AppSettings["appName"];

        //NLog purpose
        private static Logger logger = LogManager.GetLogger("file");
        #endregion

        #region "Methods"
        /// <summary>
        /// Create Google Contacts with name and mobile number
        /// </summary>
        /// <param name="name"></param>
        /// <param name="number"></param>
        public static string CreateContacts(string name, long number)
        {
            string[] scopes = new string[] { "https://www.googleapis.com/auth/contacts" };     // view your basic profile info.
            string result = string.Empty;
            Response response = new Response();
            try
            {
                // Use the current Google .net client library to get the Oauth2 stuff.
                UserCredential credential = GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets { ClientId = clientId, ClientSecret = clientSecret, }
                                                                                             , scopes
                                                                                             , "default"
                                                                                             , CancellationToken.None
                                                                                             , new FileDataStore("test")).Result;


                // Translate the Oauth permissions to something the old client libray can read
                OAuth2Parameters parameters = new OAuth2Parameters();
                parameters.AccessToken = credential.Token.AccessToken;
                parameters.RefreshToken = credential.Token.RefreshToken;

               result =  CreateGooleContacts(parameters, name, Convert.ToString(number));
            }
            catch (Exception ex)
            {
                //Log the Exception
                logger.Error($"Error Occured: {ex.ToString()}");

                //Frame the result
                response.StatusCode = 500;
                response.result = ex;

                //Final result
                result = JsonConvert.SerializeObject(response);
            }
            return result;
        }

        /// <summary>
        /// create new contacts based on OAuth parameters
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="name"></param>
        /// <param name="number"></param>
        public static string CreateGooleContacts(OAuth2Parameters parameters, string name, string number)
        {
            Response response = new Response();
            string result = string.Empty;

            try
            {
                RequestSettings settings = new RequestSettings(appName, parameters);
                ContactsRequest cr = new ContactsRequest(settings);
                Contact newEntry = new Contact();

                //Set the contact's name.
                newEntry.Name = new Google.GData.Extensions.Name()
                {
                    FullName = name,
                };

                // Set the contact's phone numbers.
                newEntry.Phonenumbers.Add(new Google.GData.Extensions.PhoneNumber()
                {
                    Primary = true,
                    Rel = ContactsRelationships.IsMobile,
                    Value = number,
                });

                // Insert the contact.
                Uri feedUri = new Uri(ContactsQuery.CreateContactsUri("default"));
                Contact createdEntry = cr.Insert(feedUri, newEntry);

                //Log the result
                logger.Info($"Contact: {createdEntry.Name.FullName} Created");

                //Frame the result
                response.StatusCode = 200;
                response.result = createdEntry;

                //Final result
                result = JsonConvert.SerializeObject(response);
            }
            catch (Exception ex)
            {
                //Log the Exception
                logger.Error($"Error Occured: {ex.ToString()}");

                //Frame the result
                response.StatusCode = 500;
                response.result = ex;

                //Final result
                result = JsonConvert.SerializeObject(response);
            }
            return result;
        }

        /// <summary>
        /// Get All contacts
        /// </summary>
        /// <param name="cr"></param>
        public void PrintDateMinQueryResults(ContactsRequest cr)
        {
            Response response = new Response();
            string result = string.Empty;

            try
            {
                ContactsQuery query = new ContactsQuery(ContactsQuery.CreateContactsUri("default"));
                Feed<Contact> feed = cr.Get<Contact>(query);
                foreach (Contact contact in feed.Entries)
                {
                    if (contact.Phonenumbers.Count > 0)
                        Console.WriteLine(contact.Name.FullName);
                    Console.WriteLine(contact.Phonenumbers[0].Value);
                }

                //Frame the result
                response.StatusCode = 200;
                response.result = feed.Entries;

                //Final result
                result = JsonConvert.SerializeObject(response);
            }
            catch(Exception ex)
            {
                //Log the Exception
                logger.Error($"Error Occured: {ex.ToString()}");

                //Frame the result
                response.StatusCode = 500;
                response.result = ex;

                //Final result
                result = JsonConvert.SerializeObject(response);
            }
            
        }
        #endregion
    }
}
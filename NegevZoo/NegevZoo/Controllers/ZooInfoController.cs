﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;
using DAL;
using DAL.Models;

namespace NegevZoo.Controllers
{
    public class ZooInfoController : ControllerBase
    {
        #region Price

        /// <summary>
        /// Gets all the Price with data in the given language.
        /// </summary>
        /// <param name="language">The data language. Default is Hebrew</param>
        /// <returns>All Prices with that language.</returns>
        [HttpGet]
        [Route("prices/all/{language}")]
        public IEnumerable<Price> GetAllPrices(int language = 1)
        {
            try
            {
                using (var db = GetContext())
                {
                    return db.GetAllPrices(language);
                }

            }
            catch (Exception Exp)
            {
                Logger.GetInstance(isTesting).WriteLine(Exp.Message, Exp.StackTrace, "Language: " + language);
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }
        }

        /// <summary>
        /// Adds or Updates the Price element.
        /// </summary>
        /// <param name="price">The element to add or update</param>
        [HttpPost]
        [Route("prices/update")]
        public void UpdatePrice(Price price)
        {
            try
            {
                using (var db = GetContext())
                {
                    if (ValidateSessionId(db))
                    {
                        db.UpdatePrice(price);
                    }
                    else
                    {
                        throw new AuthenticationException("Couldn't validate the session");
                    }
                }

            }
            catch (Exception Exp)
            {
                var priceInput = "Id: " + price.id + ", population: " + price.population +
                    ", price: " + price.pricePop + ", language: " + price.language;

                Logger.GetInstance(isTesting).WriteLine(Exp.Message, Exp.StackTrace, "Price: " + priceInput);
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }
        }

        /// <summary>
        /// Deletes the Price element.
        /// </summary>
        /// <param name="priceId">The element's priceId to delete</param>
        [HttpDelete]
        [Route("prices/delete/{priceId}")]
        public void DeletePrice(int priceId)
        {
            try
            {
                using (var db = GetContext())
                {
                    if (ValidateSessionId(db))
                    {
                        db.DeletePrice(priceId);
                    }
                    else
                    {
                        throw new AuthenticationException("Couldn't validate the session");
                    }
                }

            }
            catch (Exception Exp)
            {
                Logger.GetInstance(isTesting).WriteLine(Exp.Message, Exp.StackTrace, "Id: " + priceId);
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }
        }

        #endregion

        #region Opening hours

        /// <summary>
        /// This method is for the visitors application
        /// Gets all the OpeningHoursResults elements with data in the given language.
        /// </summary>
        /// <param name="language">The data language. Default is Hebrew</param>
        /// <returns>All OpeninngHourResults elements with that language.</returns>
        [HttpGet]
        [Route("OpeningHours/all/{language}")]
        public IEnumerable<OpeningHourResult> GetAllOpeningHourResults(int language = 1)
        {
            try
            {
                using (var db = GetContext())
                {
                    return db.GetAllOpeningHours(language);
                }

            }
            catch (Exception Exp)
            {
                Logger.GetInstance(isTesting).WriteLine(Exp.Message, Exp.StackTrace, "Language: " + language);
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }
        }
        
        /// <summary>
        /// This method is for the workers site.
        /// Gets all the OpeningHours elements in hebrew and days as int.
        /// </summary>
        /// <returns>All OpeninngHour elements in hebrew.</returns>
        [HttpGet]
        [Route("OpeningHours/type/all")]
        public IEnumerable<OpeningHour> GetAllOpeningHoursType()
        {
            try
            {
                using (var db = GetContext())
                {
                    return db.GetAllOpeningHoursType();
                }

            }
            catch (Exception Exp)
            {
                Logger.GetInstance(isTesting).WriteLine(Exp.Message, Exp.StackTrace);
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }
        }
        
        /// <summary>
        /// Adds or Updates the OpeningHour element.
        /// </summary>
        /// <param name="openingHour">The element to add or update</param>
        [HttpPost]
        [Route("OpeningHours/update")]
        public void UpdateOpeningHour(OpeningHour openingHour)
        {
            try
            {
                using (var db = GetContext())
                {
                    if (ValidateSessionId(db))
                    {
                        db.UpdateOpeningHour(openingHour);
                    }
                    else
                    {
                        throw new AuthenticationException("Couldn't validate the session");
                    }
                }
            }
            catch (Exception Exp)
            {
                var openingInput = "Id: " + openingHour.id + ", day: " + openingHour.day +
                    ", start time: "+ openingHour.startTime + ", end time: "+ openingHour.endTime +
                    ", language: "+ openingHour.language;

                Logger.GetInstance(isTesting).WriteLine(Exp.Message, Exp.StackTrace, "Opening Hour: " + openingInput);
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }
        }
        
        /// <summary>
        /// Deletes the OpeningHour element from all the languages.
        /// </summary>
        /// <param name="openHourId">The element's openHourId to delete</param>
        [HttpDelete]
        [Route("OpeningHours/delete/{openHourId}")]
        public void DeleteOpeningHour(int openHourId)
        {
            try
            {
                using (var db = GetContext())
                {
                    if (ValidateSessionId(db))
                    {
                        db.DeleteOpeningHour(openHourId);
                    }
                    else
                    {
                        throw new AuthenticationException("Couldn't validate the session");
                    }
                }

            }
            catch (Exception Exp)
            {
                Logger.GetInstance(isTesting).WriteLine(Exp.Message, Exp.StackTrace, "Id: " + openHourId);
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }
        }

        #endregion

        #region Contact Info

        /// <summary>
        /// Gets all the ContactInfo elements with data in the given language.
        /// </summary>
        /// <param name="language">The data language. Default is Hebrew</param>
        /// <returns>All ContactInfo elements with that language.</returns>
        [HttpGet]
        [Route("ContactInfos/all/{language}")]
        public IEnumerable<ContactInfo> GetAllContactInfos(int language = 1)
        {
            try
            {
                using (var db = GetContext())
                {
                    return db.GetAllContactInfos(language);
                }
            }
            catch (Exception Exp)
            {
                Logger.GetInstance(isTesting).WriteLine(Exp.Message, Exp.StackTrace, "Language: " + language);
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }
        }

        /// <summary>
        /// Adds or Updates the ContactInfo element.
        /// </summary>
        /// <param name="contactInfo">The element to add or update</param>
        [HttpPost]
        [Route("ContactInfos/update")]
        public void UpdateContactInfo(ContactInfo contactInfo)
        {
            try
            {
                using (var db = GetContext())
                {
                    if (ValidateSessionId(db))
                    {
                        db.UpdateContactInfo(contactInfo);
                    }
                    else
                    {
                        throw new AuthenticationException("Couldn't validate the session");
                    }
                }
            }
            catch (Exception Exp)
            {
                var contactInput = "Id: " + contactInfo.id + ", via: " + contactInfo.via + 
                    ", address: " + contactInfo.address +", lanaguage: "+ contactInfo.language;

                Logger.GetInstance(isTesting).WriteLine(Exp.Message, Exp.StackTrace, "Contact info: " + contactInput);
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }
        }

        /// <summary>
        /// Deletes the ContactInfo element.
        /// </summary>
        /// <param name="contactId">The ContactInfo element's id to delete</param>
        [HttpDelete]
        [Route("ContactInfos/delete/{contactId}")]
        public void DeleteContactInfo(int contactId)
        {
            try
            {
                using (var db = GetContext())
                {
                    if (ValidateSessionId(db))
                    {
                        db.DeleteContactInfo(contactId);
                    }
                    else
                    {
                        throw new AuthenticationException("Couldn't validate the session");
                    }
                }
            }
            catch (Exception Exp)
            {
                Logger.GetInstance(isTesting).WriteLine(Exp.Message, Exp.StackTrace, "Id: " + contactId);
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }
        }

        #endregion

        #region Special events

        /// <summary>
        /// Gets all the SpecialEvent elements with data in that language.
        /// </summary>
        /// <param name="language">The data language. Default is Hebrew</param>
        /// <returns>All SpecialEvent elements with that language.</returns>
        [HttpGet]
        [Route("SpecialEvents/all/{language}")]
        public IEnumerable<SpecialEvent> GetAllSpecialEvents(int language = 1)
        {
            try
            {
                using (var db = GetContext())
                {
                    return db.GetAllSpecialEvents(language);
                }
            }
            catch (Exception Exp)
            {
                Logger.GetInstance(isTesting).WriteLine(Exp.Message, Exp.StackTrace, "Language: " + language);
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }
        }
        
        /// <summary>
        /// Adds or Updates the SpecialEvent element.
        /// </summary>
        /// <param name="specialEvent">The element to add or update</param>
        /// <param name="isPush">This parameter states if the operation should send push notification</param>
        [HttpPost]
        [Route("SpecialEvents/update/{isPush}")]
        public void UpdateSpecialEvent(SpecialEvent specialEvent, bool isPush)
        {
            try
            {
                using (var db = GetContext())
                {
                    if (ValidateSessionId(db))
                    {
                        db.UpdateSpecialEvent(specialEvent, isPush);
                    }
                    else
                    {
                        throw new AuthenticationException("Couldn't validate the session");
                    }
                }

            }
            catch (Exception Exp)
            {
                var specialInput = "Id: " + specialEvent.id + ", title: " + specialEvent.title + 
                    ", description: " + specialEvent.description + ", start date: " + specialEvent.startDate + 
                    ", end date: " + specialEvent.endDate + ", image Url: " + specialEvent.imageUrl + 
                    ", language: " + specialEvent.language + ", is push: " +isPush;

                Logger.GetInstance(isTesting).WriteLine(Exp.Message, Exp.StackTrace, "Special Event:" + specialInput);
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }
        }

        /// <summary>
        /// Deletes the SpecialEvent element.
        /// </summary>
        /// <param name="specialEventId">The SpecialEvent element's id to delete</param>
        [HttpDelete]
        [Route("SpecialEvents/delete/{specialEventId}")]
        public void DeleteSpecialEvent(int specialEventId)
        {
            try
            {
                using (var db = GetContext())
                {
                    if (ValidateSessionId(db))
                    {
                        db.DeleteSpecialEvent(specialEventId);
                    }
                    else
                    {
                        throw new AuthenticationException("Couldn't validate the session");
                    }
                }
            }
            catch (Exception Exp)
            {
                Logger.GetInstance(isTesting).WriteLine(Exp.Message, Exp.StackTrace, "Id: " + specialEventId);
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }
        }

        [HttpPost]
        [Route("SpecialEvents/upload")]
        public IHttpActionResult SpecialEventsImagesUpload()
        {
            var httpRequest = HttpContext.Current.Request;
            if (httpRequest.Files.Count < 1)
            {
                return BadRequest();
            }

            try
            {
                using (var db = GetContext())
                {
                    if (!ValidateSessionId(db))
                    {
                        throw new AuthenticationException("Couldn't validate the session");
                    }
                    var uploadedImages = db.FileUpload(httpRequest, @"~/assets/specialEvents/");
                    return Ok(uploadedImages);
                }
            }
            catch (Exception Exp)
            {
                Logger.GetInstance(isTesting).WriteLine(Exp.Message, Exp.StackTrace);
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }
        }
        #endregion

        #region Wall Feed

        /// <summary>
        /// Gets all the WallFeed with data in that language.
        /// </summary>
        /// <param name="language">The data language. Default is Hebrew</param>
        /// <returns>All WallFeeds with that language.</returns>
        [HttpGet]
        [Route("Wallfeed/all/{language}")]
        public IEnumerable<WallFeed> GetAllFeeds(int language = 1)
        {
            try
            {
                using (var db = GetContext())
                {
                    return db.GetAllWallFeeds(language);
                }
            }
            catch (Exception Exp)
            {
                Logger.GetInstance(isTesting).WriteLine(Exp.Message, Exp.StackTrace, "Langauge: " + language);
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }
        }

        /// <summary>
        /// Add or updates the WallFeed.
        /// </summary>
        /// <param name="feed">The WallFeed to add or update</param>
        /// <param name="isPush">is the feed need to be pushed</param>
        /// <param name="isWallFeed"> is the feed should be added to the feed wall</param>
        [HttpPost]
        [Route("Wallfeed/update/{isPush}/{isWallFeed}")]
        [Route("Wallfeed/update/{isPush}/{isWallFeed}/{pushRecipients}")]
        public void UpdateWallFeed(WallFeed feed, bool isPush, bool isWallFeed, string pushRecipients = null)
        {
            try
            {
                using (var db = GetContext())
                {
                    if (ValidateSessionId(db))
                    {
                        feed.created = DateTime.Today;
                        db.UpdateWallFeed(feed, isPush, isWallFeed, pushRecipients);
                    }
                    else
                    {
                        throw new AuthenticationException("Couldn't validate the session");
                    }
                }
            }
            catch (Exception Exp)
            {
                var feedInput = "Id: " + feed.id + ", title: " + feed.title + ", info: " + feed.info + 
                    ", created: " + feed.created + ", langauge: " + feed.language + ", is push "+ isPush + 
                    ", is wall feed: " + isWallFeed;
                Logger.GetInstance(isTesting).WriteLine(Exp.Message, Exp.StackTrace);
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }
        }

        /// <summary>
        /// Removes a WallFeed.
        /// </summary>
        /// <param name="feedId">The WallFeed's id to deletes</param>
        [HttpDelete]
        [Route("Wallfeed/delete/{feedId}")]
        public void DeleteWallFeed(int feedId)
        {
            try
            {
                using (var db = GetContext())
                {
                    if (ValidateSessionId(db))
                    {
                        db.DeleteWallFeed(feedId);
                    }
                    else
                    {
                        throw new AuthenticationException("Couldn't validate the session");
                    }
                }
            }
            catch (Exception Exp)
            {
                Logger.GetInstance(isTesting).WriteLine(Exp.Message, Exp.StackTrace, "Id: " + feedId);
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }
        }

        #endregion
        
        #region General Info

        /// <summary>
        /// Gets the zoo's about info.
        /// </summary>
        /// <param name="language">The data language. Default is Hebrew</param>
        /// <returns>The zoo's about info.</returns>
        [HttpGet]
        [Route("about/info/{language}")]
        public AboutUsResult GetZooAboutInfo(int language = 1)
        {
            try
            {
                using (var db = GetContext())
                {
                    var aboutUs = db.GetZooAboutInfo(language);
                    return new AboutUsResult()
                    {
                        AboutUs = aboutUs
                    };
                    //return db.GetZooAboutInfo(language)
                    //    .Select(zi =>
                    //        new AboutUsResult
                    //        {
                    //            AboutUs = zi
                    //        })
                    //    .ToArray();
                }
            }
            catch (Exception Exp)
            {
                Logger.GetInstance(isTesting).WriteLine(Exp.Message, Exp.StackTrace, "Language: " + language);
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }
        }

        /// <summary>
        /// Adds or Updates the zoo's about info.
        /// </summary>
        /// <param name="language">The data language. Default is Hebrew</param>
        /// <param name="info">The info to add or update</param>
        [HttpPost]
        [Route("about/update/{language}")]
        public void UpdateZooAboutInfo(AboutUsResult aboutUs, int language = 1)
        {
            try
            {
                using (var db = GetContext())
                {
                    if (ValidateSessionId(db))
                    {
                        db.UpdateZooAboutInfo(aboutUs.AboutUs, language);
                    }
                    else
                    {
                        throw new AuthenticationException("Couldn't validate the session");
                    }
                }
            }
            catch (Exception Exp)
            {
                Logger.GetInstance(isTesting).WriteLine(Exp.Message, Exp.StackTrace, "About info: " + aboutUs.AboutUs + ", language: " + language);
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }
        }

        /// <summary>
        /// Gets the zoo's openingHourNote.
        /// </summary>
        /// <param name="language">The data language. Default is Hebrew</param>
        /// <returns>The zoo's opening hour note.</returns>
        [HttpGet]
        [Route("openingHours/openingHourNote/{language}")]
        public OpeningHourNoteResult GetOpeningHourNote(int language = 1)
        {
            try
            {
                using (var db = GetContext())
                {
                    return new OpeningHourNoteResult { OpeningHourNote = db.GetOpeningHourNote(language) };
                }
            }
            catch (Exception Exp)
            {
                Logger.GetInstance(isTesting).WriteLine(Exp.Message, Exp.StackTrace, "Language: " + language);
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }
        }

        /// <summary>
        /// Adds or Updates the zoo's openingHourNote.
        /// </summary>
        /// <param name="note">The note to add or update</param>
        /// <param name="language">The data language. Default is Hebrew</param>
        [HttpPost]
        [Route("openingHours/update/{language}")]
        public void UpdateOpeningHourNote(OpeningHourNoteResult openingHourNote, int language = 1)
        {
            try
            {
                using (var db = GetContext())
                {
                    if (ValidateSessionId(db))
                    {
                        db.UpdateOpeningHourNote(openingHourNote.OpeningHourNote, language);
                    }
                    else
                    {
                        throw new AuthenticationException("Couldn't validate the session");
                    }
                }
            }
            catch (Exception Exp)
            {
                Logger.GetInstance(isTesting).WriteLine(Exp.Message, Exp.StackTrace, "Opening Hour note: " + openingHourNote.OpeningHourNote + ", langauge: " + language);
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }
        }

        /// <summary>
        /// Gets the zoo's ContactInfoNote.
        /// </summary>
        /// <param name="language">The data language. Default is Hebrew</param>
        /// <returns>The zoo's Contact us note.</returns>
        [HttpGet]
        [Route("contactInfos/contactInfoNote/{language}")]
        public ContactInfoNoteResult GetContactInfoNote(int language = 1)
        {
            try
            {
                using (var db = GetContext())
                {
                    return new ContactInfoNoteResult { ContactInfoNote = db.GetContactInfoNote(language) };
                }
            }
            catch (Exception Exp)
            {
                Logger.GetInstance(isTesting).WriteLine(Exp.Message, Exp.StackTrace, "Language: " + language);
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }
        }

        /// <summary>
        /// Adds or Updates the zoo's ContactInfoNote.
        /// </summary>
        /// <param name="note">The note to add or update</param>
        /// <param name="language">The data language. Default is Hebrew</param>
        [HttpPost]
        [Route("contactInfos/update/{language}")]
        public void UpdateContactInfoNote(ContactInfoNoteResult contactInfoNote, int language = 1)
        {
            try
            {
                using (var db = GetContext())
                {
                    if (ValidateSessionId(db))
                    {
                        db.UpdateContactInfoNote(contactInfoNote.ContactInfoNote, language);
                    }
                    else
                    {
                        throw new AuthenticationException("Couldn't validate the session");
                    }
                }
            }
            catch (Exception Exp)
            {
                Logger.GetInstance(isTesting).WriteLine(Exp.Message, Exp.StackTrace, "Contact info note: " + contactInfoNote.ContactInfoNote + ", language: " + language);
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }
        }
        
        /// <summary>
        /// Gets the zoo's map url.
        /// </summary>
        /// <returns>The map relative path.</returns>
        [HttpGet]
        [Route("map/url")]
        public IEnumerable<MapResult> GetMapUrl()
        {
            try
            {
                using (var db = GetContext())
                {
                    return db.GetMapUrl()
                        .Select(zi =>
                            new MapResult
                            {
                                Url = zi
                            })
                        .ToArray();
                }
            }
            catch (Exception Exp)
            {
                Logger.GetInstance(isTesting).WriteLine(Exp.Message, Exp.StackTrace);
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }
        }
        #endregion
        
        #region Languages
        /// <summary>
        /// Gets all the Languages.
        /// </summary>
        /// <returns>All The Languages.</returns>
        [HttpGet]
        [Route("languages/all")]
        public IEnumerable<Language> GetAllLanguages()
        {
            try
            {
                using (var db = GetContext())
                {
                    return db.GetAllLanguages();
                }

            }
            catch (Exception Exp)
            {
                Logger.GetInstance(isTesting).WriteLine(Exp.Message, Exp.StackTrace);
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }
        }
        #endregion

        #region ModelClasses
        
        //This inner class is so we will be able to return a primitive object via http get
        public class AboutUsResult
        {
            public String AboutUs { get; set; }
        }

        public class ContactInfoNoteResult
        {
            public String ContactInfoNote { get; set; }
        }

        public class OpeningHourNoteResult
        {
            public String OpeningHourNote { get; set; }
        }

        #endregion

        /// <summary>
        /// Gets all the relevant info for the application.
        /// </summary>
        /// <returns>All the relevant info for the application.</returns>
        [HttpGet]
        [Route("app/all/{language}")]
        public IHttpActionResult GetAllInfo(int language)
        {
            try
            {
                using (var db = this.GetContext())
                {
                    return Ok(db.GetAllInfo(language));
                }
            }
            catch (Exception Exp)
            {
                Logger.GetInstance(isTesting).WriteLine(Exp.Message, Exp.StackTrace, "Language: " + language);
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }
        }
    }
}

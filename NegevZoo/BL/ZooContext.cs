﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using DAL;
using DAL.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BL
{
    public class ZooContext : IDisposable 
    {
        private IZooDB zooDB;

        public ZooContext(bool isTesting = true)
        {
            try
            {
                if (isTesting)
                {
                    zooDB = zooDB ?? DummyDB.GetInstance();
                }
                else
                {
                    zooDB = new NegevZooDBEntities();
                }
            }
            catch (Exception exp)
            {
                throw new Exception("Could not connect to the database");
            }
        }

        public ZooContext(IZooDB db)
        {
            zooDB = db;
        }

        #region Enclosure

        /// <summary>
        /// Gets the enclosures results.
        /// </summary>
        /// <param name="language">The enclosure's data language.</param>
        /// <returns>The enclosures.</returns>
        public IEnumerable<EnclosureResult> GetAllEnclosureResults(int language)
        {
            if (!ValidLanguage(language))
            {
                throw new ArgumentException("Wrong input. Wrong language.");
            }

            var enclosures = zooDB.GetAllEnclosures();
            var enclosureDetails = zooDB.GetAllEnclosureDetails().Where(e => e.language == language);

            var enclosureResults = from e in enclosures
                                   join ed in enclosureDetails on e.id equals ed.encId
                                   select new EnclosureResult
                                   {
                                       Id                   = e.id,
                                       Language             = ed.language,
                                       MarkerIconUrl        = e.markerIconUrl,
                                       MarkerLatitude       = e.markerLatitude,
                                       MarkerLongtitude     = e.markerLongitude,
                                       PictureUrl           = e.pictureUrl,
                                       Name                 = ed.name,
                                       Story                = ed.story
                                   };

            return enclosureResults.ToArray();
        }
        
        /// <summary>
        /// Gets the enclosure by id.
        /// </summary>
        /// <param name="language">The enclosure's data language.</param>
        /// <param name="id">The enclosure's id.</param>
        /// <returns>The enclosure.</returns>
        /// 
        public EnclosureResult GetEnclosureById(int id, int language)
        {
            //validate atrributes
            if (!ValidLanguage(language))
            {
                throw new ArgumentException("Wrong input. Wrong language.");
            }

            Enclosure enc = zooDB.GetAllEnclosures().SingleOrDefault(e => e.id == id);
            
            //check that the enclosure exists
            if (enc == null)
            {
                throw new ArgumentException("Wrong input. enclosure id doesn't exists");
            }

            EnclosureDetail details = zooDB.GetAllEnclosureDetails().SingleOrDefault(e => e.encId == id && e.language == language);

            //in case that there isn't data in the wanted language than taking the hebrew data.
            if (details == null)
            {
                details = zooDB.GetAllEnclosureDetails().SingleOrDefault(e => e.encId == id && e.language == GetHebewLanguage());
            }

            var enclosureResult = new EnclosureResult
            {
                Id = enc.id,
                Name = details.name,
                Story = details.story,
                MarkerLatitude = enc.markerLatitude,
                MarkerLongtitude = enc.markerLongitude,
                MarkerIconUrl = enc.markerIconUrl,
                PictureUrl = enc.pictureUrl,
                Language = details.language
            };

            return enclosureResult; 
        }
        
        /// <summary>
        /// Gets the enclosure by it's name.
        /// </summary>
        /// <param name="language">The enclosure's data language.</param>
        /// <param name="name">The enclosure's name.</param>
        /// <returns>The enclosure.</returns>
        public IEnumerable<EnclosureResult> GetEnclosureByName(string name, int language)
        {
            var enclosureResults = new List<EnclosureResult>();

            if (!ValidLanguage(language))
            {
                throw new ArgumentException("Wrong input. Wrong language.");
            }

            var encDetails = zooDB.GetAllEnclosureDetails().Where(e => e.name.Contains(name));

            // it there are details in the name and language
            if (encDetails != null)
            {
                foreach(EnclosureDetail details in encDetails)
                {
                    enclosureResults.Add(GetEnclosureById((int)details.encId ,language));
                }

            }

            return enclosureResults;
        }

        /// <summary>
        /// Gets the enclosure by longtitude and latitude.
        /// </summary>
        /// <param name="language">The enclosure's data language.</param>
        /// <param name="longtitud">The enclosure's longtitude.</param>
        /// <param name="latitude">The enclosure's latitude.</param>
        /// <returns>The enclosure.</returns>
        public Enclosure GetEnclosureByPosition(double longtitud, double latitude, int language)
        {
            return zooDB.GetAllEnclosures().SingleOrDefault(e => /*e.language == language &&*/
                                                            (e.markerLongitude <= longtitud + 5 && e.markerLongitude >= longtitud - 5) &&
                                                            (e.markerLatitude <= latitude + 5 && e.markerLatitude >= latitude - 5) );
        }

        /// <summary>
        /// Gets the enclosures types results.
        /// </summary>
        /// <returns>The enclosures types .</returns>
        public IEnumerable<Enclosure> GetAllEnclosures()
        {
            return zooDB.GetAllEnclosures();
        }

        /// <summary>
        /// Gets the enclosure details by the enclosure type id.
        /// </summary>
        /// <param name="encId">The enclosure's type id.</param>
        /// <returns>The enclosure details in all the languages.</returns>
        public IEnumerable<EnclosureDetail> GetEnclosureDetailsById(int encId)
        {
            return zooDB.GetAllEnclosureDetails().Where(e => e.encId == encId);
        }

        /// <summary>
        /// Gets the enclosure's recurring events by it's id.
        /// </summary>
        /// <param name="language">The enclosure's data language.</param>
        /// <param name="encId">The enclosure's id.</param>
        /// <returns>The enclosure's recurring events.</returns>
        public IEnumerable<RecurringEvent> GetRecurringEvents (int encId, int language)
        {
            if (!ValidLanguage(language))
            {
                throw new ArgumentException("Wrong input. Wrong language.");
            }

            var enc = zooDB.GetAllEnclosures().SingleOrDefault(e => e.id == encId);

            if(enc == null)
            {
                throw new ArgumentException("Wrong input. enclosure id doesn't exists");
            }

            return zooDB.GetAllRecuringEvents().Where(re => re.language == language && re.enclosureId == encId).ToList();
        }
   
        /// <summary>
        /// Gets the enclosure's pictures by it's id.
        /// </summary>
        /// <param name="encId">The enclosure's id.</param>
        /// <returns>The enclosure's pictures events.</returns>
        public IEnumerable<EnclosurePicture> GetEnclosurePicturesById(int encId)
        {
            //check that the enclosure exists
            if (!GetAllEnclosures().Any(e => e.id == encId))
            {
                throw new ArgumentException("Wrong input. The enclosure doesn't exists");
            }

            return zooDB.GetAllEnclosurePictures().Where(e => e.enclosureId == encId);
        }

        /// <summary>
        /// Gets the enclosure's videos by it's id.
        /// </summary>
        /// <param name="encId">The enclosure's id.</param>
        /// <returns>The enclosure's videos events.</returns>
        public IEnumerable<YoutubeVideoUrl> GetEnclosureVideosById(int encId)
        {
            //check that the enclosure exists
            if (!GetAllEnclosures().Any(e => e.id == encId))
            {
                throw new ArgumentException("Wrong input. The enclosure doesn't exists");
            }

            return zooDB.GetAllEnclosureVideos().Where(e => e.enclosureId == encId);
        }

        /// <summary>
        /// Gets the recurring events.
        /// </summary>
        /// <param name="language">The RecurringEvent's data language.</param>
        /// <returns>The RecurringEvents.</returns>
        public IEnumerable<RecurringEvent> GetAllRecurringEvents(int language)
        {
            if (!ValidLanguage(language))
            {
                throw new ArgumentException("Wrong input. Wrong language.");
            }

            return zooDB.GetAllRecuringEvents().Where(gr => gr.language == language);
        }

        /// <summary>
        /// Updates The enclosure.
        /// </summary>
        /// <param name="enclosures">The enclosures to update.</param>
        public void UpdateEnclosure(Enclosure enclosure)
        {
            //validate enclosure attributes
            //0. Exists.
            if (enclosure == default(Enclosure))
            {
                throw new ArgumentException("No enclosure given.");
            }

            //1. enclosure name
            if (IsEmptyString(enclosure.name) || IsNullOrWhiteSpace(enclosure.name))
            {
                throw new ArgumentException("Wrong input. enclosure name is empty or white space");
            }

            //TODO: add a check to latitude or longtitude out of the range of the zoo.

            var enclosures = zooDB.GetAllEnclosures();

            if (enclosure.id == default(int)) //add a new enclosure
            {
                if (enclosures.Any(en => en.name == enclosure.name))
                {
                    throw new ArgumentException("Wrong input in adding enclosure. Name already exists");
                }
                
                enclosures.Add(enclosure);
            }
            else //update existing enclosure
            {
                Enclosure oldEnc = enclosures.SingleOrDefault(en => en.id == enclosure.id);

                //check that the enclosure exists
                if (oldEnc == null)
                {
                    throw new ArgumentException("Wrong input. Enclosure id doesn't exits");
                }

                // check that if the name changed, it doesn't exits
                if (oldEnc.name != enclosure.name && enclosures.Any(en => en.name == enclosure.name))//The name changed
                {
                    throw new ArgumentException("Wrong input in updating enclosure. Name already exsits");
                }

                //enclosure.id = oldEnc.id;
                oldEnc.markerIconUrl = enclosure.markerIconUrl;
                oldEnc.markerLatitude = enclosure.markerLatitude;
                oldEnc.markerLongitude = enclosure.markerLongitude;
                oldEnc.name = enclosure.name;
                oldEnc.pictureUrl = enclosure.pictureUrl;
            }
        }

        /// <summary>
        /// Updates or adds The enclosure details.
        /// </summary>
        /// <param name="enclosureDetail">The enclosureDetails to update.</param>
        public void UpdateEnclosureDetails(EnclosureDetail enclosureDetail)
        {
            //validate enclosure details attributes
            //0. Exists.
            if (enclosureDetail == default(EnclosureDetail) || enclosureDetail.encId == default(int))
            {
                throw new ArgumentException("No enclosure given.");
            }

            //1. validate language
            if (!ValidLanguage((int)enclosureDetail.language))
            {
                throw new ArgumentException("Wrong input. Wrong language.");
            }

            //2. enclosure detail name
            if (IsEmptyString(enclosureDetail.name) || IsNullOrWhiteSpace(enclosureDetail.name))
            {
                throw new ArgumentException("Wrong input. enclosure detail name is empty or white space");
            }
            
            //3. check that the enclosure id exists
            if (!GetAllEnclosures().Any(e => e.id == enclosureDetail.encId))
            {
                throw new ArgumentException("Wrong input. Enclosure detail id doesn't exists..");
            }

            var enclosuresDetails = zooDB.GetAllEnclosureDetails();

            EnclosureDetail oldEnc = enclosuresDetails.SingleOrDefault(en => en.encId == enclosureDetail.encId && en.language == enclosureDetail.language);

            if (oldEnc == null) //add a new enclosure details
            {
                if (enclosuresDetails.Any(ed => ed.name == enclosureDetail.name))
                {
                    throw new ArgumentException("Wrong input. The name of the enclosure alreay exists.");
                }
                enclosuresDetails.Add(enclosureDetail);
            }
            else //update an exists enclosure detail
            //get the existing enclosure detail
            {
                // check that if the name changed, it doesn't exits
                if (oldEnc.name != enclosureDetail.name && enclosuresDetails.Any(en => en.name == enclosureDetail.name))//The name changed
                {
                    throw new ArgumentException("Wrong input in updating enclosure. Name already exsits");
                }

                oldEnc.language = enclosureDetail.language;
                oldEnc.name = enclosureDetail.name;
                oldEnc.story = enclosureDetail.story;
            }
        }
        
        /// <summary>
        /// Updates or adds The enclosure picture.
        /// </summary>
        /// <param name="enclosurePicture">The enclosures to update.</param>
        public void UpdateEnclosurePicture(EnclosurePicture enclosurePicture)
        {
            //validate attributes
            //0. Exists
            if (enclosurePicture == default(EnclosurePicture))
            {
                throw new ArgumentException("No EnclosurePicture given");
            }

            //1. check that the enclosure exists
            if (!GetAllEnclosures().Any(e => e.id == enclosurePicture.enclosureId))
            {
                throw new ArgumentException("Wrong input. Enclosure doesn't exists");
            }

            if (IsEmptyString(enclosurePicture.pictureUrl) || IsNullOrWhiteSpace(enclosurePicture.pictureUrl))
            {
                throw new ArgumentException("Wrong input. The url is empty or white spaces");
            }

            var allEnclosurePictures = zooDB.GetAllEnclosurePictures();

            if (enclosurePicture.id == default(int)) // add a new enclosure picture
            {
                if (allEnclosurePictures.Any(p => p.pictureUrl == enclosurePicture.pictureUrl))
                {
                    throw new ArgumentException("Wrong input while adding enclosure picture. The picture url already exists.");
                }

                allEnclosurePictures.Add(enclosurePicture);
            }
            else //update an existsing picture
            {
                var oldPic = allEnclosurePictures.SingleOrDefault(ep => ep.id == enclosurePicture.id);

                if (oldPic == null)
                {
                    throw new ArgumentException("Wrong input. There is no EnclosurePicture doesn't exists");
                }

                if (oldPic.pictureUrl != enclosurePicture.pictureUrl && allEnclosurePictures.Any(ep => ep.pictureUrl == enclosurePicture.pictureUrl))
                {
                    throw new ArgumentException("Wrong input while updating enclosure picture. The picture url is already exists");
                }

                oldPic.pictureUrl = enclosurePicture.pictureUrl;
                oldPic.enclosureId = enclosurePicture.enclosureId;
            }
        }

        /// <summary>
        /// Updates or adds The enclosure video.
        /// </summary>
        /// <param name="enclosureVideo">The enclosures to update.</param>
        public void UpdateEnclosureVideo(YoutubeVideoUrl enclosureVideo)
        {
            //validate attributes
            //0.Exists
            if (enclosureVideo == default(YoutubeVideoUrl))
            {
                throw new ArgumentException("No video given");
            }
            
            //1. check that the enclosure exists
            if (!GetAllEnclosures().Any(e => e.id == enclosureVideo.enclosureId))
            {
                throw new ArgumentException("Wrong input. Enclosure doesn't exists");
            }

            //2. check the url
            if (IsEmptyString(enclosureVideo.videoUrl) || IsNullOrWhiteSpace(enclosureVideo.videoUrl))
            {
                throw new ArgumentException("Wrong input. The url is empty or white spaces");
            }

            var allEnclosureVideos = zooDB.GetAllEnclosureVideos();

            if (enclosureVideo.id == default(int)) //add video
            {
                if (allEnclosureVideos.Any(v => v.videoUrl == enclosureVideo.videoUrl))
                {
                    throw new ArgumentException("Wrong input while adding enclosure video. The enclosure vido url already exists");
                }

                allEnclosureVideos.Add(enclosureVideo);
            }
            else //update video
            {
                var oldVideo = allEnclosureVideos.SingleOrDefault(v => v.id == enclosureVideo.id);

                if (oldVideo == null)
                {
                    throw new ArgumentException("Wrong input. Video doesn't exists");
                }

                if (oldVideo.videoUrl != enclosureVideo.videoUrl && allEnclosureVideos.Any(v => v.videoUrl == enclosureVideo.videoUrl))
                {
                    throw new ArgumentException("Wrong input while updating enclosure video. The enclosure vido url already exists");
                }

                oldVideo.videoUrl = enclosureVideo.videoUrl;
                oldVideo.enclosureId = enclosureVideo.enclosureId;
            }
        }
        
        /// <summary>
        /// Adds or updates a recurring event element.
        /// </summary>
        /// <param name="recEvent">The RecurringEvent element to update or add.</param>
        public void UpdateRecurringEvent(RecurringEvent recEvent)
        {
            //validate attributes
            //0.Exists
            if (recEvent == default(RecurringEvent))
            {
                throw new ArgumentException("No RecurringEvent was given");
            }

            //1. validate language
            if (!ValidLanguage((int)recEvent.language))
            {
                throw new ArgumentException("Wrong input. Wrong language.");
            }

            //2. check that the enclosure exists
            if (!GetAllEnclosures().Any(e => e.id == recEvent.enclosureId))
            {
                throw new ArgumentException("Wrong input. Enclosure doesn't exists");
            }

            //3. check the description
            if (IsEmptyString(recEvent.description) || IsNullOrWhiteSpace(recEvent.description))
            {
                throw new ArgumentException("Wrong input. The descrioption is empty or white spaces");
            }

            //4. check the day
            if (Enum.ToObject(typeof(Days), recEvent.day) == null || recEvent.day / 10 != (recEvent.language - 1))
            {
                throw new ArgumentException("Wrong input. The day is not defined");
            }

            //5. check the time
            if (recEvent.endTime.Subtract(recEvent.startTime) <= TimeSpan.Zero)
            {
                throw new ArgumentException("Wrong input. The start time is bigger or equal to the end time");
            }

            var allRecurringEvents = zooDB.GetAllRecuringEvents();

            //TODO: Add Notification!
            if (recEvent.id == default(int)) //add recurring event
            {
                //check that there isn't other Recurring event to this enclosure in the same time.
                if (allRecurringEvents.Any(re => re.enclosureId == recEvent.enclosureId && ValidateTime(re, recEvent)))
                {
                    throw new ArgumentException("Wrong input while adding recurring event. There is another recurring event in the same time");
                }

                allRecurringEvents.Add(recEvent);
            }
            else //update RecurringEvent
            {
                var oldRecEvent = allRecurringEvents.SingleOrDefault(re => re.id == recEvent.id);

                if (recEvent == null)
                {
                    throw new ArgumentException("Wrong input. RecurringEvent doesn't exists");
                }

                if (allRecurringEvents.Any(re => re.enclosureId == recEvent.enclosureId && ValidateTime(re, recEvent)))
                {
                    throw new ArgumentException("Wrong input while updating enclosure video. The enclosure vido url already exists");
                }

                oldRecEvent.enclosureId = recEvent.enclosureId;
                oldRecEvent.day         = recEvent.day;
                oldRecEvent.description = recEvent.description;
                oldRecEvent.startTime   = recEvent.startTime;
                oldRecEvent.endTime     = recEvent.endTime;
            }
            
        }

        /// <summary>
        /// Delete The enclosure.
        /// </summary>
        /// <param name="id">The enclosure's id to delete.</param>
        public void DeleteEnclosure(int id)
        {
            Enclosure enclosure = zooDB.GetAllEnclosures().SingleOrDefault(e => e.id == id);

            //Check that can delete the enclosure
            //1.enclosure exists
            if (enclosure == null)
            {
                throw new ArgumentException("Wrong input. enclosure ID doesn't exists.");
            }

            //2. exists animals
            if (zooDB.GetAllAnimals().Any(an => an.enclosureId == id))
            {
                throw new InvalidOperationException("Threre are animals that related to this enclosure");
            }

            //3. exists recurring events
            if (zooDB.GetAllRecuringEvents().Any(re => re.enclosureId == id))
            {
                throw new InvalidOperationException("Threre are recurring events that related to this enclosure");
            }

            var toRemove = zooDB.GetAllEnclosureDetails().Where(ed => ed.encId == enclosure.id).ToList();

            zooDB.GetAllEnclosureDetails().RemoveRange(toRemove);
            zooDB.GetAllEnclosures().Remove(enclosure);
        }

        /// <summary>
        /// Delete The enclosure picture.
        /// </summary>
        /// <param name="enclosurePictureId">The EnclosurePicture's id to delete.</param>
        public void DeleteEnclosurePicture(int enclosurePictureId)
        {
            //check that the enclosure picture exists
            var enclosurePicure = zooDB.GetAllEnclosurePictures().SingleOrDefault(e => e.id == enclosurePictureId);

            if (enclosurePicure == null){
                throw new ArgumentException("Wrong input. The enclsure doesn't exists");
            }
            
            zooDB.GetAllEnclosurePictures().Remove(enclosurePicure);
        }

        /// <summary>
        /// Delete The enclosure video.
        /// </summary>
        /// <param name="enclosureVideoId">The EnclosureVideo's id to delete.</param>
        public void DeleteEnclosureVideo(int enclosureVideoId)
        {
            //check that the enclosure exists
            var enclosureVideo = zooDB.GetAllEnclosureVideos().SingleOrDefault(e => e.enclosureId == enclosureVideoId);

            if (enclosureVideo == null)
            {
                throw new ArgumentException("Wrong input. The enclsure doesn't exists");
            }

            zooDB.GetAllEnclosureVideos().Remove(enclosureVideo);
        }
        
        /// <summary>
        /// Delete The recurringEvent.
        /// </summary>
        /// <param name="id">The RecurringEvent's id to delete.</param>
        public void DeleteRecurringEvent(int id)
        {
            RecurringEvent recEvent = zooDB.GetAllRecuringEvents().SingleOrDefault(re => re.id == id);

            if (recEvent == null)
            {
                throw new ArgumentException("Wrong input. RecurringEvent's id doesn't exists.");
            }

            zooDB.GetAllRecuringEvents().Remove(recEvent);
        }
        
        #endregion

        #region Animals
        
        /// <summary>
        /// Gets all the animals results.
        /// </summary>
        /// <param name="language">The animal's data language.</param>
        /// <returns>All the AnimalResults with the given language.</returns>
        public IEnumerable<AnimalResult> GetAnimalsResults(int language)
        {
            if (!ValidLanguage(language))
            {
                throw new ArgumentException("Wrong input. Wrong language.");
            }
            var animals = zooDB.GetAllAnimals();
            var animalsDetails = zooDB.GetAllAnimalsDetails();

            var animalResults = from a in animals
                                join ad in animalsDetails on new { a.id, language = (long)language } equals new { id = ad.animalId, ad.language }
                                select new AnimalResult
                                {
                                    Id              = a.id,
                                    Name            = ad.name,
                                    Story           = ad.story,
                                    EncId           = a.enclosureId,
                                    Category        = ad.category,
                                    Series          = ad.series,
                                    Family          = ad.family,
                                    Distribution     = ad.distribution,
                                    Reproduction    = ad.reproduction,
                                    Food            = ad.food,
                                    Preservation    = a.preservation,
                                    PictureUrl      = a.pictureUrl,
                                    Language        = ad.language
                                };

            return animalResults.ToArray();
        }

        /// <summary>
        /// This method gets all the AnimalResults that have a speicsial stories.
        /// </summary>
        /// <param name="language">The data's language</param>
        /// <returns>All the AnimalResults that have a special story in the given language.</returns>
        public IEnumerable<AnimalResult> GetAnimalResultsWithStory(int language)
        {
            return GetAnimalsResults(language).Where(ar => !String.IsNullOrWhiteSpace(ar.Story));
        }

        /// <summary>
        /// Gets animal by Id and language.
        /// </summary>
        /// <param name="id">The animal's Id.</param>
        /// <param name="language">The data's language</param>
        /// <returns>The animal.</returns>
        public AnimalResult GetAnimalById(int id, int language)
        {
            if (!ValidLanguage(language))
            {
                throw new ArgumentException("Wrong input. Wrong language.");
            }

            Animal an = zooDB.GetAllAnimals().SingleOrDefault(a => a.id == id);

            if (an == null)
            {
                throw new ArgumentException("Wrong input. animal id doesn't exsits");
            }

            AnimalDetail details = zooDB.GetAllAnimalsDetails().SingleOrDefault(ad => ad.animalId == id && ad.language == language);

            //in case that there isn't data in the wanted language than taking the hebrew data.
            if (details == null)
            {
                details = zooDB.GetAllAnimalsDetails().SingleOrDefault(ad => ad.animalId == id && ad.language == GetHebewLanguage());
            }

            var animalResult = new AnimalResult
            {
                Id = id,
                Name = details.name,
                Story = details.story,
                EncId = an.enclosureId,
                Category = details.category,
                Distribution = details.distribution,
                Family = details.family,
                Food = details.food,
                Preservation = an.preservation,
                Reproduction = details.reproduction,
                Series = details.series,
                PictureUrl = an.pictureUrl,
                Language = details.language
            };

            return animalResult;
        }
        
        /// <summary>
        /// Gets animals by enclosure Id and language.
        /// </summary>
        /// <param name="encId">The enclosure's Id.</param>
        /// <param name="language">The data's language</param>
        /// <returns>The AnimalResults of animals that are in the enclosure.</returns>
        public IEnumerable<AnimalResult> GetAnimalsByEnclosure(long encId, long language)
        {
            //validate the attributes
            //0. check the language
            if (!ValidLanguage((int)language))
            {
                throw new ArgumentException("Wrong input. Wrong language.");
            }

            //1. check if the enclosure exists
            if (GetAllEnclosures().SingleOrDefault(en => en.id == encId) == null)
            {
                throw new ArgumentException("Wrong input. The enclosure doesn't exists");
            }

            //get all the animals in the enclosure.
            IEnumerable<Animal> allanimals = GetAllAnimals().Where(a => a.enclosureId == encId).ToArray();

            //initiates the answer.
            List<AnimalResult> animalsResult = new List<AnimalResult>();

            foreach (Animal an in allanimals)
            {
                //foreaach animal get it's AnimalResult by id (if it doesn't exists returns in hebrew.
                animalsResult.Add(GetAnimalById((int)an.id, (int)language));
            }

            return animalsResult;
        }

        /// <summary>
        /// Gets all the animals that their name conatins the given name in the wanted language.
        /// </summary>
        /// <param name="language">The data's language</param>
        /// <param name="name">The animal's name.</param>
        /// <returns>The AnimalResult.</returns>
        public IEnumerable<AnimalResult> GetAnimalByName(string name, int language)
        {
            var animalResult = new List<AnimalResult>();

            if (!ValidLanguage(language))
            {
                throw new ArgumentException("Wrong input. Wrong language.");
            }

            var anDetails = zooDB.GetAllAnimalsDetails().Where(ad => ad.name.Contains(name) && ad.language == language);

            // it there are details in the name and language
            if (anDetails != null)
            {
                foreach (AnimalDetail details in anDetails)
                {
                    animalResult.Add(GetAnimalById((int)details.animalId, language));
                }

            }

            return animalResult;
        }

        /// <summary>
        /// Gets all the animals types.
        /// </summary>
        /// <returns>The animals types.</returns>
        public IEnumerable<Animal> GetAllAnimals()
        {
            return zooDB.GetAllAnimals();
        }

        /// <summary>
        /// Gets the AnimalDetials of the animsl with the given id.
        /// </summary>
        /// <param name="animalId">The animal's id.</param>
        /// <returns>The AnimalDetails in all the languages.</returns>
        public IEnumerable<AnimalDetail> GetAllAnimalsDetailById(int animalId)
        {
            //validate the attributes
            //0. check that the animal id exists.
            if (!GetAllAnimals().Any(an => an.id == animalId))
            {
                throw new ArgumentException("Wrong input. The animal id doesn't exists");
            }

            return zooDB.GetAllAnimalsDetails().Where(an => an.animalId == animalId);
        }
        
        /// <summary>
        /// This method adds or updates the animal.
        /// </summary>
        /// <param name="animal">The animal to update.</param>
        public void UpdateAnimal(Animal animal)
        {
            //validate enclosure attributes
            //0. Exists.
            if (animal == default(Animal))
            {
                throw new ArgumentException("No animal given.");
            }

            //1. aniaml name
            if (IsEmptyString(animal.name) || IsNullOrWhiteSpace(animal.name)) 
            {
                throw new ArgumentException("Wrong input. Animal name is empty or null");
            }

            //2. enclosure exists
            if (GetEnclosureById((int)animal.enclosureId, (int)GetHebewLanguage() ) == null)
            {
                throw new ArgumentException("Wrong input. Enclosure id doesn't exists");
            }

            var animals = zooDB.GetAllAnimals();

            if (animal.id == default(int)) //add a new aniaml
            {
                // check that the name doesn't exists
                if (animals.Any(an => an.name == animal.name))
                {
                    throw new ArgumentException("Wrong input in adding animal. Animal name already exists");
                }

                animals.Add(animal);
            }
            else // update existing animal.
            {
                Animal oldAnimal = animals.SingleOrDefault(an => an.id == animal.id);

                //check that the animal exists
                if (oldAnimal == null)
                {
                    throw new ArgumentException("Wrong input. Animal id does'nt exits");
                }

                // check that id the name changed, it doesn't exists.
                if (oldAnimal.name != animal.name && animals.Any(an => an.name == animal.name))
                {
                    throw new ArgumentException("Wrong input in updating animal. Animal name already exitst");
                }

                oldAnimal.name = animal.name;
                oldAnimal.pictureUrl = animal.pictureUrl;
                oldAnimal.preservation = animal.preservation;
                oldAnimal.enclosureId = animal.enclosureId;
            }
        }

        /// <summary>
        /// This method adds or updates the given AnimalDetail.
        /// </summary>
        /// <param name="animalDetails">The object to add or update.</param>
        public void UpdateAnimalDetails(AnimalDetail animalDetails)
        {
            //validate enclosure attributes
            //0. Exists.
            if (animalDetails == default(AnimalDetail))
            {
                throw new ArgumentException("No animal given.");
            }

            //1. language
            if (!ValidLanguage((int)animalDetails.language))
            {
                throw new ArgumentException("Wrong input. Wrong language.");
            }
            
            //2. name
            if (String.IsNullOrWhiteSpace(animalDetails.name))
            {
                throw new ArgumentException("Wrong input. The name is null or whitespace.");
            }

            //3. check that the animal id exists
            if (!GetAllAnimals().Any(a => a.id == animalDetails.animalId))
            {
                throw new ArgumentException("Wrong input. The animal id coesnt exists.");
            }
            var allAnimalDetails = zooDB.GetAllAnimalsDetails();

            var oldDetails = allAnimalDetails.SingleOrDefault(ad => ad.language == animalDetails.language && ad.animalId == animalDetails.animalId);
            
            if (oldDetails == null) //add a new aniamlDetails
            {
                //TODO: check if this assertion is a must.
                // check that the name doesn't exists
                if (allAnimalDetails.Any(an => an.name == animalDetails.name))
                {
                    throw new ArgumentException("Wrong input in adding animal. Animal name already exists");
                }

                allAnimalDetails.Add(animalDetails);
            }
            else // update existing animal.
            {
                // check that id the name changed, it doesn't exists.
                if (oldDetails.name != animalDetails.name && allAnimalDetails.Any(an => an.name == animalDetails.name))
                {
                    throw new ArgumentException("Wrong input in updating animal. Animal name already exitst");
                }

                oldDetails.name = animalDetails.name;
                oldDetails.story = animalDetails.story;
                oldDetails.series = animalDetails.series;
                oldDetails.reproduction = animalDetails.reproduction;
                oldDetails.language = animalDetails.language;
                oldDetails.food = animalDetails.food;
                oldDetails.family = animalDetails.family;
                oldDetails.distribution = animalDetails.distribution;
                oldDetails.category = animalDetails.category;
            }
        }

        /// <summary>
        /// Delete the animal.
        /// </summary>
        /// <param name="id">The animal's id to delete.</param>
        public void DeleteAnimal(int id)
        {
            Animal animal = zooDB.GetAllAnimals().SingleOrDefault(a => a.id == id);
            
            //check that the animal exists
            if (animal == null)
            {
                throw new ArgumentException("Wrong input. Animal doesn't exists");
            }

            var animalsToDelete = zooDB.GetAllAnimalsDetails().Where(ad => ad.animalId == animal.id).ToList();

            zooDB.GetAllAnimalsDetails().RemoveRange(animalsToDelete);
            zooDB.GetAllAnimals().Remove(animal);
        }

        #endregion

        #region Zoo Info

        #region Prices
        
        /// <summary>
        /// Gets all the Price elements.
        /// </summary>
        /// <param name="language">The Price's data language.</param>
        /// <returns>The prices entitiess.</returns>
        public IEnumerable<Price> GetAllPrices(int language)
        {
            //validate the attributes.
            //1. check the lanuguage
            if (!ValidLanguage(language))
            {
                throw new ArgumentException("Wrong input. Wrong language");
            }

            return zooDB.GetAllPrices().Where(p => p.language == language).ToArray();
        }
        
        /// <summary>
        /// Adds or update the Price elements.
        /// </summary>
        /// <param name="price">The Price to add or update.</param>
        public void UpdatePrice(Price price)
        {
            //Validate the price attribute
            //0. Exists
            if (price == default(Price))
            {
                throw new ArgumentException("No price given.");
            }

            //1. check that the population is valid
            if (String.IsNullOrWhiteSpace(price.population) || String.IsNullOrEmpty(price.population))
            {
                throw new ArgumentException("Wrong input. The price population is empty or null");
            }
            
            //2. check that the price amount is valid
            if (price.pricePop < 0)
            {
                throw new ArgumentException("Wrong input, The price amount is lower than 0");
            }

            //3. check the language
            if (!ValidLanguage((int)price.language))
            {
                throw new ArgumentException("Wrong Input. Wrong Language");
            }
            
            var prices = zooDB.GetAllPrices();

            if (price.id == default(int)) //need to add the price.
            {
                if (prices.Any(p => p.population == price.population))
                {
                    throw new ArgumentException("Wrong input while adding price. Price population already exists.");
                }

                prices.Add(price);
            }
            else //update existing price
            {
                var oldPrice = prices.SingleOrDefault(p => p.id == price.id);

                //check that the price exists
                if (oldPrice == null)
                {
                    throw new ArgumentException("Wrong input. The price id doesn't exist");
                }

                //check that if the population changed, the new population doesn't exists.
                if (price.population != oldPrice.population && prices.Any(p => p.population == price.population))
                {
                    throw new ArgumentException("Wrong input while updating price. Price population already exists");
                }

                oldPrice.language = price.language;
                oldPrice.population = price.population;
                oldPrice.pricePop = price.pricePop;
            }
        }

        /// <summary>
        /// Delete the Price elements.
        /// </summary>
        /// <param name="id">The Price's id to delete.</param>
        public void DeletePrice(int id)
        {
            Price price = zooDB.GetAllPrices().SingleOrDefault(p => p.id == id);

            if (price == null)
            {
                throw new ArgumentException("Wrong input. Price doesn't exists.");
            }

            zooDB.GetAllPrices().Remove(price);
        }
        #endregion

        #region OpeningHours
        /// <summary>
        /// Gets all the OpeningHourResults elements - days as strings.
        /// </summary>
        /// <param name="language">The OpeningHour's data language.</param>
        /// <returns>All the OpeningHour elemtents as days as string.</returns>
        public IEnumerable<OpeningHourResult> GetAllOpeningHours (int language)
        {
            if (!ValidLanguage(language))
            {
                throw new ArgumentException("Wrong input. Wrong language");
            }

            var openingHours = zooDB.GetAllOpeningHours().Where(oh => oh.language == language).ToArray();

            List<OpeningHourResult> opHoursResults = new List<OpeningHourResult>();

            foreach(OpeningHour oh in openingHours)
            {
                opHoursResults.Add(new OpeningHourResult
                {
                    Id = oh.id,
                    Day = Enum.GetName(typeof(Days), oh.day),
                    EndTime = oh.endTime,
                    StartTime = oh.startTime,
                    Language = oh.language
                });
            }

            return opHoursResults;
        }

        /// <summary>
        /// Gets all the OpeningHour elements - days as int.
        /// </summary>
        /// <returns>All the OpeningHour elemtents as days as int.</returns>
        public IEnumerable<OpeningHour> GetAllOpeningHoursType()
        {
            return zooDB.GetAllOpeningHours().Where(oh => oh.language == GetHebewLanguage()).ToArray();
        }

        /// <summary>
        /// Adds or update the OpeningHour element.
        /// </summary>
        /// <param name="OpeningHour">The OpeningHour element to add or update.</param>
        public void UpdateOpeningHour(OpeningHour openingHour)
        {
            //validate opening hour attributs
            //0. Exists
            if (openingHour == default(OpeningHour))
            {
                throw new ArgumentException("No OpeningHour was given");
            }

            //1. check the day
            if (openingHour.day < 1 || openingHour.day > 7)
            {
                throw new ArgumentException("Wrong input. The day is empty or null");
            }

            //2. check that the end is after the open
            if (openingHour.startTime.CompareTo(openingHour.endTime) > 0)
            {
                throw new ArgumentException("Wrong input. The start time is later than the end time.");
            }

            var openingHours = zooDB.GetAllOpeningHours();

            if (openingHour.id == default(int)) //add a new opening hour
            {
                if (openingHours.Any(oh => oh.day == openingHour.day))
                {
                    throw new ArgumentException("Wrong input while adding Opening hour. The day of the opening hour is already exsists");
                }

                //add the hebrew OpeningHour
                openingHour.language = GetHebewLanguage();
                openingHours.Add(openingHour);

                // add the other 3
                int day = openingHour.day;

                for (int i = 1; i <= 3; i++)
                {
                    var oh = new OpeningHour
                    {
                        day = i * 10 + day,
                        startTime = openingHour.startTime,
                        endTime = openingHour.endTime,
                        language = i+1
                    };

                    openingHours.Add(oh);
                }
            }
            else //update exsist opening hour
            {
                var oldEntity = openingHours.SingleOrDefault(oh => oh.id == openingHour.id);

                if (oldEntity == null)
                {
                    throw new ArgumentException("Wrong input. The opening hour id doesn't exists.");
                }

                var oldHours = openingHours.Where(oh => (oh.day-oldEntity.day) % 10 == 0).ToList();

                oldHours.OrderBy(o => o.language);

                for (int i = 0; i <= 3; i++)
                {

                    if (i >= oldHours.Count())
                    {
                        break;
                    }

                    OpeningHour oldHour = oldHours.ElementAt(i);

                    //check that if the day changed than the new day doesnt exists.
                    if (oldHour.language == GetHebewLanguage() && oldHour.day != openingHour.day && openingHours.Any(o => o.day == openingHour.day))
                    {
                        throw new ArgumentException("Wrong input while updating Opening hour. The Opening hour day already exists");
                    }

                    openingHours.Remove(oldHour);

                    OpeningHour oh = new OpeningHour
                    {
                        day = i * 10 + openingHour.day,
                        startTime = openingHour.startTime,
                        endTime = openingHour.endTime,
                        language = oldHour.language
                    };

                    openingHours.Add(oh);
                }
            }
        }

        /// <summary>
        /// Delete the OpeningHour elements from all the languages.
        /// </summary>
        /// <param name="id">The OpeningHour's id to delete.</param>
        public void DeleteOpeningHour(int id)
        {
            //gets all the OpeningHour elements from the db.
            var allOpeningHours = zooDB.GetAllOpeningHours();

            //get the relevan OpeningHour
            OpeningHour openingHour = allOpeningHours.SingleOrDefault(oh => oh.id == id);
            
            //validate the attributes.
            //1. check that the OpeningHour of the given id exists .
            if (openingHour == null)
            {
                throw new ArgumentException("Wrong input. Opening hour id doesn't exsists.");
            }

            int day = openingHour.day;

            //delete from all the languages
            for (int i = 0; i <= 3; i++)
            {
                //openingHour.day = i * 10 + day;
                var toDelete = allOpeningHours.SingleOrDefault(oh => oh.day == i * 10 + day);
                if (toDelete != null)
                {
                    allOpeningHours.Remove(toDelete);
                }
            }
        }

        #endregion

        #region ContatInfo
        /// <summary>
        /// Gets all the ContactInfos elements.
        /// </summary>
        /// <param name="language">The ContactInfo's data language.</param>
        /// <returns>All the ContactInfos elemtents.</returns>
        public IEnumerable<ContactInfo> GetAllContactInfos(int language)
        {
            if (!ValidLanguage(language))
            {
                throw new ArgumentException("Wrong input. Wrong language");
            }

            return zooDB.GetAllContactInfos().Where(ci => ci.language == language).ToArray();
        }

        /// <summary>
        /// Adds or update the ContactInfo element.
        /// </summary>
        /// <param name="contactInfo">The ContactInfo element to add or update.</param>
        public void UpdateContactInfo(ContactInfo contactInfo)
        {
            //validate contact info attributs
            //0. Exists
            if (contactInfo == default(ContactInfo))
            {
                throw new ArgumentException("No ContactInfo was given");
            } 

            //1. check that the address is valid
            if (String.IsNullOrWhiteSpace(contactInfo.address) || String.IsNullOrEmpty(contactInfo.address))
            {
                throw new ArgumentException("Wrong input. ContactInfo's address is empty or null");
            }

            //2. check that the via is valid
            if (String.IsNullOrWhiteSpace(contactInfo.via) || String.IsNullOrEmpty(contactInfo.via))
            {
                throw new ArgumentException("Wrong input. ContactInfo's via is empty or null");
            }

            //3. check the language
            if (!ValidLanguage((int)contactInfo.language))
            {
                throw new ArgumentException("Wrong input. Wrong language");
            }

            var contactInfos = zooDB.GetAllContactInfos();

            if (contactInfo.id == default(int)) // add a new contact info
            {
                //check that the address and via doesn't exists
                if (contactInfos.Any(ci => ci.via == contactInfo.via && ci.address == contactInfo.address))
                {
                    throw new ArgumentException("Wrong input while adding contactInfo. Contact info address and Via already exists.");
                }

                contactInfos.Add(contactInfo);
            }
            else //update existing contact info
            {
                ContactInfo oldContact = contactInfos.SingleOrDefault(ci => ci.id == contactInfo.id);

                //check that the contact info exists
                if (oldContact == null)
                {
                    throw new ArgumentException("Wrong input. The ContactInfo's id doesn't exists");
                }

                //check that id the via or address change there isn't exsisting via and address
                if ((oldContact.via != contactInfo.via || oldContact.address != contactInfo.address) && contactInfos.Any(ci => ci.address == contactInfo.address && ci.via == contactInfo.via))
                {
                    throw new ArgumentException("Wrong input while updating contactInfo. Contact info address and Via already exists.");
                }

                oldContact.language = contactInfo.language;
                oldContact.via = contactInfo.via;
                oldContact.address = contactInfo.address;
            }
        }

        /// <summary>
        /// Delete the ContactInfo elements.
        /// </summary>
        /// <param name="id">The ContactInfo's id to delete.</param>
        public void DeleteContactInfo(int id)
        {
            ContactInfo contactInfo = zooDB.GetAllContactInfos().SingleOrDefault(ci => ci.id == id);

            if (contactInfo == null)
            {
                throw new ArgumentException("Wrong input. ContactInfo id doesn't exists");
            }

            zooDB.GetAllContactInfos().Remove(contactInfo);
        }

        #endregion

        #region Special Events
        
        /// <summary>
        /// Gets all the SpecialEvent elements.
        /// </summary>
        /// <param name="language">The SpecialEvent's data language.</param>
        /// <returns>All the SpecialEvent elemtents.</returns>
        public IEnumerable<SpecialEvent> GetAllSpecialEvents(int language)
        {
            if (!ValidLanguage(language))
            {
                throw new ArgumentException("Wrong input. Wrong language.");
            }

            return zooDB.GetAllSpecialEvents().Where(se => se.language == language).ToArray();
        }

        /// <summary>
        /// Gets SpecialEvent elements between two dates.
        /// </summary>
        /// <param name="language">The SpecialEvent's data language.</param>
        /// <param name="startDate">The start date to look for</param>
        /// <param name="endDate">The end date to look for</param>
        /// <returns>All the SpecialEvent elemtents.</returns>
        public IEnumerable<SpecialEvent> GetSpecialEventsByDate(DateTime startDate, DateTime endDate, int language)
        {
            if (!ValidLanguage(language))
            {
                throw new ArgumentException("Wrong input. Wrong language.");
            }

            if (DateTime.Compare(endDate,startDate) <= 0)
            {
                throw new ArgumentException("Wrong input. the end date is sooner than the start date");
            }

            return zooDB.GetAllSpecialEvents().Where(se => se.language == language && 
                                                     se.startDate >= startDate &&
                                                     se.endDate <= endDate).ToArray();
        }

        /// <summary>
        /// Adds or update the SpecialEvents element.
        /// </summary>
        /// <param name="specialEvent">The SpecialEvent element to add or update.</param>
        public void UpdateSpecialEvent(SpecialEvent specialEvent, bool isPush)
        {
            //validate SpecialEvent attributs
            //0. Exists
            if (specialEvent == default(SpecialEvent))
            {
                throw new ArgumentException("No SpecialEvent was given");
            }

            //1. check that the dates are good
            if (DateTime.Compare(specialEvent.endDate,specialEvent.startDate) < 0)
            {
                throw new ArgumentException("Wrong input. The end date is earlier than the start date.");
            }

            //2. check the language
            if (!ValidLanguage((int)specialEvent.language))
            {
                throw new ArgumentException("Wrong input. Wrong language");
            }

            //3. check the title
            if (String.IsNullOrWhiteSpace(specialEvent.title))
            {
                throw new ArgumentException("Wrong input. The title is null or white space");
            }

            //4. check the description
            if (String.IsNullOrWhiteSpace(specialEvent.description))
            {
                throw new ArgumentException("Wrong input. The description is null or white space");
            }

            var specialEvents = zooDB.GetAllSpecialEvents();

            if (specialEvent.id == default(int)) //add a new special event
            {
                //check that the description and title doesn't exists together.
                if (specialEvents.Any(se => se.description == specialEvent.description && se.title == specialEvent.title))
                {
                    throw new ArgumentException("Wrong input while adding a SpecialEvent. The SpecialEvent description already exists");
                }

                specialEvents.Add(specialEvent);
            }
            else //update existing SpecialEvent
            {
                var oldEvent = specialEvents.SingleOrDefault(se => se.id == specialEvent.id);

                //check that the event exists.
                if (oldEvent == null)
                {
                    throw new ArgumentException("Wrong input. The SpecialEvent's id doesn't exists");
                }

                //check that if the description or title changed than they doesn't already exists together
                if ((oldEvent.description != specialEvent.description || oldEvent.title != specialEvent.title) && specialEvents.Any(se => se.description == specialEvent.description && se.title == specialEvent.title))
                {
                    throw new ArgumentException("Wrong input While updating SpecialEvent. The SpecialEvent descroption already exists.");
                }

                oldEvent.description    = specialEvent.description;
                oldEvent.title          = specialEvent.title;
                oldEvent.startDate      = specialEvent.startDate;
                oldEvent.endDate        = specialEvent.endDate;
                oldEvent.imageUrl       = specialEvent.imageUrl;
                oldEvent.language       = specialEvent.language;
            }
            
            if (isPush)
            {
                SendNotificationsAllDevices(specialEvent.title, specialEvent.description);
            }
        }

        /// <summary>
        /// Delete the SpecialEvent elements.
        /// </summary>
        /// <param name="id">The SpecialEvent's id to delete.</param>
        public void DeleteSpecialEvent(int id)
        {
            SpecialEvent specialEvent = zooDB.GetAllSpecialEvents().SingleOrDefault(se => se.id == id);

            if (specialEvent == null)
            {
                throw new ArgumentException("Wrong input. SpecialEvent's id doesn't exists");
            }
            zooDB.GetAllSpecialEvents().Remove(specialEvent);
        }

        #endregion

        #region Wall Feed
        
        /// <summary>
        /// Gets all the feed walls messages.
        /// </summary>
        /// <param name="language">The wallfeed's data language</param>
        /// <returns>The WallFeeds messages.</returns>
        public IEnumerable<WallFeed> GetAllWallFeeds(int language)
        {
            if (!ValidLanguage(language))
            {
                throw new ArgumentException("Wrong input. Wrong language");
            }

            return zooDB.GetAllWallFeeds().Where(e => e.language == language).ToArray();
        }

        /// <summary>
        /// Adds or Updates a feed wall message.
        /// </summary>
        /// <param name="feed">The wallfeed to add or update</param>
        /// <param name="isPush">represents if the feed should be send as push notification</param>
        /// <param name="isWallFeed">represents if the feed should be added to the wall feed</param>
        public void UpdateWallFeed(WallFeed feed, bool isPush, bool isWallFeed)
        {
            //validate WallFeed attributs
            //0. Exists
            if (feed == default(WallFeed))
            {
                throw new ArgumentException("No wall feed was given");
            }

            //1. check that one of the boolean expressions is true
            if (!isPush && !isWallFeed)
            {
                throw new ArgumentException("The feed should be added to the wall feed or sent as push");
            }

            //2. check the title
            if (String.IsNullOrWhiteSpace(feed.title))
            {
                throw new ArgumentException("Wrong input. The title is null or white space");
            }

            //3. check the info
            if (String.IsNullOrWhiteSpace(feed.info))
            {
                throw new ArgumentException("Wrong input. The info is null or white space");
            }

            //4. check the language
            if (!ValidLanguage((int)feed.language))
            {
                throw new ArgumentException("Wrong input. Wrong language");
            }

            var wallFeeds = zooDB.GetAllWallFeeds();


            if (isWallFeed && feed.id == default(int)) //add new feed wall
            {
                //check that the info and title doesn't exists together
                if (wallFeeds.Any(wf => wf.info == feed.info && wf.title == feed.title))
                {
                    throw new ArgumentException("Wrong input while adding WallFeed. The WallFeed info and title are already exists.");
                }

                wallFeeds.Add(feed);
            }
            else //update a feed wall
            {
                WallFeed oldFeed = wallFeeds.SingleOrDefault(wf => wf.id == feed.id);

                //check that the wall feed exists
                if (oldFeed == null)
                {
                    throw new ArgumentException("Wrong input. The WallFeed's id doesn't exists");
                }

                //check that if the info ot title changed than they doesn't already exits together
                if ((oldFeed.info != feed.info || oldFeed.title != feed.title) && wallFeeds.Any(wf => wf.info == feed.info && wf.title == feed.title))
                {
                    throw new ArgumentException("Wrong input while updating WallFeed. The WallFeed Info and title are already exists");
                }

                oldFeed.language = feed.language;
                oldFeed.title = feed.title;
                oldFeed.info = feed.info;
                oldFeed.created = feed.created;
            }

            if (isPush)
            {
                SendNotificationsAllDevices(feed.title, feed.info);
            }
        }

        /// <summary>
        /// Delete a feed wall message.
        /// </summary>
        /// <param name="id">The wallfeed's id to delete</param>
        public void DeleteWallFeed(int id)
        {
            var wallFeed = zooDB.GetAllWallFeeds().SingleOrDefault(wf => wf.id == id);

            if (wallFeed == null)
            {
                throw new ArgumentException("Wrong input. WallFeed's id doesn't exists");
            }

            zooDB.GetAllWallFeeds().Remove(wallFeed);
        }

        #endregion

        #region General Info
        /// <summary>
        /// Gets the zoo's about info.
        /// </summary>
        /// <param name="language">The language the about info is in.</param>
        /// <returns>The zoo's about info.</returns>
        public IEnumerable<String> GetZooAboutInfo(int language)
        {
            if (!ValidLanguage(language))
            {
                throw new ArgumentException("Wrong input. Wrong language");
            }

            return zooDB.GetGeneralInfo()
                .Where(ge => ge.language == language)
                .Select(ge => ge.aboutUs)
                .ToArray();
        }

        /// <summary>
        /// Updates the zoo's about info.
        /// </summary>
        /// <param name="language">The language the about info is in.</param>
        /// <param name="info">The info to update.</param>
        public void UpdateZooAboutInfo(string info, int language)
        {
            //validate the AboutUs attributs
            //1. validate info
            if (String.IsNullOrWhiteSpace(info) || String.IsNullOrEmpty(info))
            {
                throw new ArgumentException("Wrong input. The info is empty or null");
            }

            //2. validate language
            if (!ValidLanguage(language))
            {
                throw new ArgumentException("Wrong input. Wrong language");
            }

            var generalInfo = zooDB.GetGeneralInfo().SingleOrDefault(gi => gi.language == language);

            generalInfo.aboutUs = info;
        }

        /// <summary>
        /// Gets the zoo's opening hours note.
        /// </summary>
        /// <param name="language">The language the note is in.</param>
        /// <returns>The zoo's openingHourNote.</returns>
        public IEnumerable<String> GetOpeningHourNote(int language)
        {
            if (!ValidLanguage(language))
            {
                throw new ArgumentException("Wrong input. Wrong language");
            }

            return zooDB.GetGeneralInfo()
                .Where(ge => ge.language == language)
                .Select(ge => ge.openingHoursNote)
                .ToArray();
        }

        /// <summary>
        /// Updates the zoo's OpeningHourNote.
        /// </summary>
        /// <param name="language">The language the note is in.</param>
        /// <param name="note">The note to update.</param>
        public void UpdateOpeningHourNote(string note, int language)
        {
            //validate the OpeningHourNote attributs
            //1. validate note
            if (String.IsNullOrWhiteSpace(note))
            {
                throw new ArgumentException("Wrong input. The note is empty or null");
            }

            //2. validate language
            if (!ValidLanguage(language))
            {
                throw new ArgumentException("Wrong input. Wrong language");
            }

            var generalInfo = zooDB.GetGeneralInfo().SingleOrDefault(gi => gi.language == language);

            generalInfo.openingHoursNote = note;
        }

        /// <summary>
        /// Gets the zoo's contact info note.
        /// </summary>
        /// <param name="language">The language the note is in.</param>
        /// <returns>The zoo's ContactInfoNote.</returns>
        public IEnumerable<String> GetContactInfoNote(int language)
        {
            if (!ValidLanguage(language))
            {
                throw new ArgumentException("Wrong input. Wrong language");
            }

            return zooDB.GetGeneralInfo()
                .Where(ge => ge.language == language)
                .Select(ge => ge.contactInfoNote)
                .ToArray();
        }

        /// <summary>
        /// Updates the zoo's contact info note.
        /// </summary>
        /// <param name="language">The language the note is in.</param>
        /// <param name="note">The note to update.</param>
        public void UpdateContactInfoNote(string note, int language)
        {
            //validate the ContactInfoNote attributs
            //1. validate note
            if (String.IsNullOrWhiteSpace(note))
            {
                throw new ArgumentException("Wrong input. The note is empty spaces or null");
            }

            //2. validate language
            if (!ValidLanguage(language))
            {
                throw new ArgumentException("Wrong input. Wrong language");
            }

            var generalInfo = zooDB.GetGeneralInfo().SingleOrDefault(gi => gi.language == language);

            generalInfo.contactInfoNote= note;
        }

        /// <summary>
        /// Gets the zoo's map url.
        /// </summary>
        /// <returns>The zoo's url map.</returns>
        public IEnumerable<String> GetMapUrl()
        {
            var language = GetAllLanguages().SingleOrDefault(l => l.name == "עברית").id;

            return zooDB.GetGeneralInfo()
                .Where(ge => ge.language == language)
                .Select(ge => ge.mapBackgroundUrl)
                .ToArray();
        }
        #endregion

        #region Languages
        public IEnumerable<Language> GetAllLanguages()
        {
            return zooDB.GetAllLanguages();
        }

        #endregion

        #endregion

        #region Map
        public MapSettingsResult GetMapSettings()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Users
        /// <summary>
        /// Gets the users.
        /// </summary>
        /// <returns>The users.</returns>
        public IEnumerable<User> GetAllUsers()
        {
            return zooDB.GetAllUsers().ToArray();
        }

        /// <summary>
        /// Connect a worker user to the system.
        /// </summary>
        /// <param name="userName">The wanted user name</param>
        /// <param name="password">The user's password</param>
        /// <returns>a boolean that indicates if the proccess succeded.</returns>
        public bool Login(string userName, string password)
        {
            User user = GetAllUsers().SingleOrDefault(u => u.name == userName);

            if (user == null)
            {
                return false;
            }

            if (VerifyMd5Hash(password + user.salt, user.password))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the user by userName and password.
        /// </summary>
        /// <param name="userName">The User name.</param>
        /// <param name="password">The User password.</param>
        /// <returns>The user.</returns>
        public User GetUserByNameAndPass(string userName, string password)
        {
            var user = zooDB.GetAllUsers().SingleOrDefault(wu => wu.password == password && wu.name == userName);

            if (user == null)
            {
                throw new ArgumentException("Can't find a user with this name and password");
            }

            return user;
        }

        /// <summary>
        /// Updates The User.
        /// </summary>
        /// <param name="userWorker">The UserWorker to add or update.</param>
        public void UpdateUser(User userWorker)
        {
            //check the attributes
            // 0.Exists
            if (userWorker == default(User))
            {
                throw new ArgumentException("No UserWorker given");
            }

            //TODO: Add an authorization test.

            // 1. Name
            if (String.IsNullOrEmpty(userWorker.name) || String.IsNullOrWhiteSpace(userWorker.name))
            {
                throw new ArgumentException("Wrong input. The user name is empty or white spaces");
            }

            // 2. password
            if (String.IsNullOrEmpty(userWorker.password) || String.IsNullOrWhiteSpace(userWorker.password))
            {
                throw new ArgumentException("Wrong input. The password is empty or white spaces");
            }

            var users = zooDB.GetAllUsers();

            if (userWorker.id == default(int)) //add a user
            {
                //check if the name already exists
                if (users.Any(wu => wu.name == userWorker.name))
                {
                    throw new ArgumentException("Wrong input while adding a User. Name already exists");
                }

                userWorker.salt = GenerateSalt();

                users.Add(userWorker);
            }
            else //update a user
            {
                var oldUser = users.SingleOrDefault(wu => wu.id == userWorker.id);

                if (oldUser == null)
                {
                    throw new ArgumentException("Wrong input. User doesn't exists");
                }

                //check if the name changed to a name that already exists
                if (oldUser.name != userWorker.name && users.Any(wu => wu.name == userWorker.name))
                {
                    throw new ArgumentException("Wrong input while updating a User. Name already exists");
                }

                oldUser.name = userWorker.name;
                oldUser.password = userWorker.password;
                userWorker.salt = GenerateSalt();
                oldUser.isAdmin = userWorker.isAdmin;
            }
        }

        /// <summary>
        /// Delete The User.
        /// </summary>
        /// <param name="id">The User's id to delete.</param>
        public void DeleteUser(int UserId)
        {
            User user = zooDB.GetAllUsers().SingleOrDefault(wu => wu.id == UserId);

            //Check that the User exists
            if (user == null)
            {
                throw new ArgumentException("Wrong input. User ID doesn't exists.");
            }

            zooDB.GetAllUsers().Remove(user);
        }

        #endregion

        #region Notification support

        /// <summary>
        /// get all the devices in the db.
        /// </summary>
        public IEnumerable<Device> GetAllDevices()
        {
            return zooDB.getAllDevices();
        }

        /// <summary>
        /// updated the status of the given device.
        /// </summary>
        /// <param name="deviceId">The device to update.</param>
        /// <param name="insidePark"> The location status of the device</param>
        public void UpdateDevice(string deviceId, bool insidePark)
        {
            //validate the attribues
            //1. check the device id.
            if (IsEmptyString(deviceId) || IsNullOrWhiteSpace(deviceId))
            {
                throw new ArgumentException("Wrong input. Device Id empty or white spaces.");
            }

            var device = zooDB.getAllDevices().SingleOrDefault(d => d.deviceId == deviceId);

            //check if the device already exists
            if (device != null)
            {
                device.lastPing = DateTime.Now;
            }
            else
            {
                device = new Device
                {
                    deviceId = deviceId,
                    insidePark = (sbyte)(insidePark? 1 : 0),
                    lastPing = DateTime.Now
                };

                zooDB.getAllDevices().Add(device);
            }

        }

        /// <summary>
        /// send notification to all the devices
        /// </summary>
        /// <param name="title">The title of the notification.</param>
        /// <param name="body">The body of the notification</param>
        public void SendNotificationsAllDevices(string title, string body)
        {
            Task.Factory.StartNew(() => NotifyAsync(title, body, true));
        }

        /// <summary>
        /// send notification to the online devices
        /// </summary>
        /// <param name="title">The title of the notification.</param>
        /// <param name="body">The body of the notification</param>
        public void SendNotificationsOnlineDevices(string title, string body)
        {
            Task.Factory.StartNew(() => NotifyAsync(title, body, false));
        }

        /// <summary>
        /// send notification to the online devices about recurring events if there are at the moment
        /// </summary>
        public void SendNotificationsOnlineDevicesRecurringEvents()
        {
            //get all the recurring events in hebrew
            //TODO: The notification will be sent only in hebrew at the moment.
            //      Need to add a method to get all the recurring events.
            //      But there is a problem with which messeage will be sent to whom
            var allRecEvents = GetAllRecurringEvents(1).ToArray();
            Console.WriteLine("Package received");
            //get the current time
            var currentTime = DateTime.Now;

            Console.WriteLine("Searching for events");
            foreach(RecurringEvent recEve in allRecEvents)
            {
                // get the difference between now and the recEve
                var timeDif = recEve.startTime.Subtract(new TimeSpan(currentTime.Hour, currentTime.Minute, currentTime.Second));
                
                //get the current day of week. add 1 because days start from 0 in c#
                var curDayOfWeek = (long)currentTime.DayOfWeek + 1;

                if (curDayOfWeek == recEve.day &&  timeDif.Hours == 0 && timeDif.Minutes <= 10 && timeDif.Minutes > TimeSpan.Zero.Minutes)
                {
                    Console.WriteLine("Event found" + recEve.title + ", ", recEve.description);
                    Task.Factory.StartNew(() => NotifyAsync(recEve.title, recEve.description, false));
                }
                else
                {
                    Console.WriteLine("No events found");
                }
            }
        }

        private async void NotifyAsync(string title, string body, bool toAll)
        {
            // TODO:: compute whether the users are online or offline and send by that.
            try
            {
                string key = Properties.Settings.Default.serverKey;
                string id = Properties.Settings.Default.senderId;
                
                var devices = new List<Device>();

                if (toAll) //the notification should be sent to all the users
                {
                    devices.AddRange(zooDB.getAllDevices().ToList());
                }
                else
                {   //the notification should be sent to the online users.
                    //TODO: check if 30 minuits is the difference between online of offline
                    devices.AddRange(zooDB.getAllDevices().Where(d => d.lastPing.Date.CompareTo(DateTime.Now.Date) == 0 && d.lastPing.AddMinutes(30)>DateTime.UtcNow.ToLocalTime()).ToList());
                }

                // Format the server's key.
                var serverKey = string.Format("key={0}", key);

                // Format the sender's Id.
                var senderId = string.Format("id={0}", id);

                // Get all registration ids from the database.
                var registration_ids = devices.Select(d => d.deviceId).ToArray();

                // Construct the request's data.
                var data = new
                {
                    registration_ids,
                    notification = new { title, body, sound = "default", vibrate = true, background = true }
                };

                var jsonBody = JsonConvert.SerializeObject(data);

                using (var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://fcm.googleapis.com/fcm/send"))
                {
                    httpRequest.Headers.TryAddWithoutValidation("Authorization", serverKey);
                    httpRequest.Headers.TryAddWithoutValidation("Sender", senderId);
                    httpRequest.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                    using (var httpClient = new HttpClient())
                    {
                        await httpClient.SendAsync(httpRequest);
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO:: LOG.
                throw ex;
            }
        }

        #endregion

        #region private functions

        private bool ValidLanguage(int language)
        {
            return Enum.IsDefined(typeof(Languages), language);
        }

        private bool ValidHour(int hour, int min)
        {
            return hour > 0 && hour < 24 && Enum.IsDefined(typeof(AvailableMinutes), min);
        }

        private long GetHebewLanguage()
        {
            return GetAllLanguages().SingleOrDefault(l => l.name == "עברית").id;
        }

        private bool IsEmptyString(string str)
        {
            return String.IsNullOrEmpty(str);
        }

        private bool IsNullOrWhiteSpace(string str)
        {
            return String.IsNullOrWhiteSpace(str);
        }

        private bool ValidateTime(RecurringEvent re, RecurringEvent recEvent)
        {
            return  (re.day == recEvent.day) &&
                    ((re.startTime.Subtract(recEvent.startTime) > TimeSpan.Zero &&
                    re.startTime.Subtract(recEvent.endTime) < TimeSpan.Zero)
                    ||
                    (re.endTime.Subtract(recEvent.startTime) > TimeSpan.Zero &&
                    re.endTime.Subtract(recEvent.endTime) < TimeSpan.Zero));
        }

        private Random random = new Random();
        private String GenerateSalt()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 32).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        bool VerifyMd5Hash(string input, string hash)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                // Hash the input.
                string hashOfInput = GetMd5Hash(input);

                // Create a StringComparer an compare the hashes.
                StringComparer comparer = StringComparer.OrdinalIgnoreCase;

                if (0 == comparer.Compare(hashOfInput, hash))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private string GetMd5Hash(string input)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                // Convert the input string to a byte array and compute the hash.
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Create a new Stringbuilder to collect the bytes
                // and create a string.
                StringBuilder sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data 
                // and format each one as a hexadecimal string.
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                // Return the hexadecimal string.
                return sBuilder.ToString();
            }
        }

        #endregion

        #region Images

        /// <summary>
        /// Resizes an image.
        /// </summary>
        /// <param name="b">The original bitmap.</param>
        /// <param name="nWidth">new width.</param>
        /// <param name="nHeight">new height.</param>
        /// <returns>A resized image.</returns>
        private Bitmap ResizeImage(Bitmap b, int nWidth, int nHeight)
        {
            Bitmap result = new Bitmap(nWidth, nHeight);

            using (Graphics g = Graphics.FromImage(result))
                g.DrawImage(b, 0, 0, nWidth, nHeight);

            return result;
        }

        /// <summary>
        /// Sets the image's orientation field to default value and flips the image correctly.
        /// </summary>
        /// <param name="b">The bitmap.</param>
        private void SetOrientationToDefault(Bitmap b)
        {
            // 0x112 Is the orientation property that windows uses to orientate the image.
            if (Array.IndexOf(b.PropertyIdList, 274) > -1)
            {
                var orientation = (int)b.GetPropertyItem(274).Value[0];
                switch (orientation)
                {
                    case 1:
                        // No rotation required.
                        break;
                    case 2:
                        b.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        break;
                    case 3:
                        b.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        break;
                    case 4:
                        b.RotateFlip(RotateFlipType.Rotate180FlipX);
                        break;
                    case 5:
                        b.RotateFlip(RotateFlipType.Rotate90FlipX);
                        break;
                    case 6:
                        b.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        break;
                    case 7:
                        b.RotateFlip(RotateFlipType.Rotate270FlipX);
                        break;
                    case 8:
                        b.RotateFlip(RotateFlipType.Rotate270FlipNone);
                        break;
                }
                // This EXIF data is now invalid and should be removed.
                b.RemovePropertyItem(274);
            }
        }

        /// <summary>
        /// Saves the image as icon.
        /// </summary>
        /// <param name="filePath">The file path to save.</param>
        private void SaveAsIcon(string filePath)
        {
            // Get the image from file.
            var image = new Bitmap(filePath);

            // Sets the image orientation to default value.
            SetOrientationToDefault(image);

            // Resize the image.
            var resizedImage = ResizeImage(image, 48, 48);

            // Dispose the original image file desc.
            image.Dispose();

            // Save the new resized image.
            resizedImage.Save(filePath);

            // Dispose the resized image file desc.
            resizedImage.Dispose();
        }
        
        #endregion

        /// <summary>
        /// post a file to the db
        /// </summary>
        /// <param name="httpRequest">The requested files.</param>
        /// <param name="relativePath">the path.</param>
        /// <returns>An array of the uploaded images path.</returns>
        public JArray FileUpload(HttpRequest httpRequest, string relativePath)
        {
            var fileNames           = new List<String>();

            foreach (string file in httpRequest.Files)
            {
                var postedFile      = httpRequest.Files[file];

                var fileExtension   = postedFile.FileName.Split('.').Last();
                var fileName        = Guid.NewGuid() + "." + fileExtension;

                var filePath        = HttpContext.Current.Server.MapPath(relativePath + fileName);

                postedFile.SaveAs(filePath);

                if (relativePath.Contains("icon") || relativePath.Contains("marker"))
                {
                    SaveAsIcon(filePath);
                }

                fileNames.Add(fileName);
            }

            var responseObject = new JArray();

            foreach (var fn in fileNames)
            {
                responseObject.Add(new JValue(relativePath.Substring(2) + fn));
            }

            return responseObject;
        }

        public void Dispose()
        {
            zooDB.SaveChanges();
        }
    }
}

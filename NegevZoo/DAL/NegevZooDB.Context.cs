﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DAL
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using DAL.Models;

    public partial class NegevZooDBEntities : IZooDB
    {
        public NegevZooDBEntities()
            : base("name=NegevZooDBEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }

        protected override List<TEntity> GetFromCache<TEntity>()
        {
            // TODO:: Fix Cache.
            throw new NotImplementedException();
        }

        protected override void SetInCache<TEntity>(List<TEntity> entity)
        {
            // TODO:: Fix Cache.
            throw new NotImplementedException();
        }

        public override DbSet<Animal> GetAllAnimals()
        {
            return Animals;
        }

        public override DbSet<Enclosure> GetAllEnclosures()
        {
            return Enclosures;
        }

        public override DbSet<RecurringEvent> GetAllRecuringEvents()
        {
            return RecurringEvents;
        }

        public override DbSet<Price> GetAllPrices()
        {
            return Prices;
        }

        public override DbSet<GeneralInfo> GetGeneralInfo()
        {
            return GeneralInfos;
        }

        public override DbSet<OpeningHour> GetAllOpeningHours()
        {
            return OpeningHours;
        }

        public override DbSet<ContactInfo> GetAllContactInfos()
        {
            return ContactInfos;
        }

        public override DbSet<SpecialEvent> GetAllSpecialEvents()
        {
            return SpecialEvents;
        }

        public override DbSet<WallFeed> GetAllWallFeeds()
        {
            return WallFeeds;
        }

        public override DbSet<MiscMarker> GetAllMiscMarkers()
        {
            return MiscMarkers;
        }

        public override DbSet<User> GetAllUsers()
        {
            return Users;
        }

        public override DbSet<Language> GetAllLanguages()
        {
            return Languages;
        }

        public override DbSet<EnclosureDetail> GetAllEnclosureDetails()
        {
            return EnclosureDetails;
        }

        public override DbSet<AnimalDetail> GetAllAnimalsDetails()
        {
            return AnimalDetails;
        }

        public override DbSet<EnclosurePicture> GetAllEnclosurePictures()
        {
            return EnclosurePictures;
        }

        public override DbSet<YoutubeVideoUrl> GetAllEnclosureVideos()
        {
            return YoutubeVideoUrls;
        }

        public override DbSet<Device> GetAllDevices()
        {
            return Devices;
        }

        public override DbSet<MapInfo> GetAllMapInfos()
        {
            return MapInfos;
        }

        public override DbSet<Route> GetAllRoutes()
        {
            return Routes;
        }

        public override DbSet<UserSession> GetAllUserSessions()
        {
            return UserSessions;
        }

        public virtual DbSet<Animal> Animals { get; set; }
        public virtual DbSet<ContactInfo> ContactInfos { get; set; }
        public virtual DbSet<EnclosurePicture> EnclosurePictures { get; set; }
        public virtual DbSet<Enclosure> Enclosures { get; set; }
        public virtual DbSet<GeneralInfo> GeneralInfos { get; set; }
        public virtual DbSet<Language> Languages { get; set; }
        public virtual DbSet<MiscMarker> MiscMarkers { get; set; }
        public virtual DbSet<OpeningHour> OpeningHours { get; set; }
        public virtual DbSet<Price> Prices { get; set; }
        public virtual DbSet<RecommendedRoute> RecommendedRoutes { get; set; }
        public virtual DbSet<RecurringEvent> RecurringEvents { get; set; }
        public virtual DbSet<SpecialEvent> SpecialEvents { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<WallFeed> WallFeeds { get; set; }
        public virtual DbSet<YoutubeVideoUrl> YoutubeVideoUrls { get; set; }
        public virtual DbSet<AnimalDetail> AnimalDetails { get; set; }
        public virtual DbSet<EnclosureDetail> EnclosureDetails { get; set; }
        public virtual DbSet<Device> Devices { get; set; }
        public virtual DbSet<MapInfo> MapInfos { get; set; }
        public virtual DbSet<Route> Routes { get; set; }
        public virtual DbSet<UserSession> UserSessions { get; set; }

    }
}

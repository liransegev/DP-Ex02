﻿using System.Collections.Generic;
using System;
using System.Text;
using System.Linq;
using FacebookWrapper;
using FacebookWrapper.ObjectModel;
using Newtonsoft.Json;
using System.IO;

namespace A16_Ex01_Shahaf_201381076_Liran_201664497
{
    public static class Controller
    {

        public static User m_LoggedInUser { get; set; }

        public static LoginResult m_Result { get; set; }

        public static bool Login()
        {
            string appID = "542892732526619";
            string[] permissions = new string[] { "user_about_me", "user_friends", "publish_actions", "user_events", "user_posts", "user_photos", "user_status" };
            FacebookWrapper.FacebookService.s_CollectionLimit = 60;

            m_Result = FacebookService.Login(appID, permissions);
            m_LoggedInUser = m_Result.LoggedInUser;
            
            return string.IsNullOrEmpty(m_Result.AccessToken) ? false : true;
        }

        public static string GetUserProfilePicture()
        {
            return m_LoggedInUser.PictureLargeURL;
        }

        public static string GetUserName()
        {
            return m_LoggedInUser.Name;
        }

        public static List<Event> GetUserEvents()
        {
            return m_LoggedInUser.Events.ToList<Event>();
        }

        public static List<Status> GetUserStatuses()
        {
            FacebookObjectCollection<Status> userStatuses = m_LoggedInUser.Statuses;
            List<Status> statuses = userStatuses.Where(s => !String.IsNullOrEmpty(s.Message)).ToList<Status>();

            return statuses;
        }

        public static List<User> GetUserFriends()
        {
            return m_LoggedInUser.Friends.ToList<User>();
        }

        public static List<Place> GetRecommendedPlaces(string i_keyWord, int i_userSelectedRadius)
        {
            FacebookObjectCollection<Checkin> userCheckins = m_LoggedInUser.Checkins;

            if (userCheckins.Count == 0)
            {
                throw new Exception("No check - ins found.");
            }

            Checkin lastCheckin = userCheckins[userCheckins.Count - 1];
            GooglePlacesApiResponse response = new GooglePlacesApiResponse();
            if (!string.IsNullOrEmpty(i_keyWord) && lastCheckin.Place != null)
            {
                Location location = lastCheckin.Place.Location;
                response = GooglePlaceAPI.GetPlaces(location.Latitude, location.Longitude, i_userSelectedRadius, i_keyWord);
                response.ChangeEncodingResponseToUTF8();
                if (response.m_ResponseStatus != "OK")
                {
                    throw new Exception("Error occur while try to get places.");
                }
            }
            else
            {
                throw new Exception("Error");
            }

            return response.m_Places;
        }

        public static SortedDictionary<int, LikesAggregation> GetAvgLikesPerHour()
        {
            MustLikesStatisticsHourUtils MustLikedPostHour = new MustLikesStatisticsHourUtils();

            return MustLikedPostHour.GetAvgLikesPerHour(m_LoggedInUser.Posts);
        }

        public static Checkin PostCheckin(string i_Place)
        {
            try
            {
                FbPlacePageID placePageId = FbPlaceAPI.getPlacePage(i_Place, m_Result);
                return m_LoggedInUser.Checkin(placePageId.m_ID);
            }
            catch (Exception)
            {
                throw new Exception("Checkin failed, try another place.");
            }
        }

        public static Status PostStatus(string i_Status)
        {
            return m_LoggedInUser.PostStatus(i_Status);
        }
    }
}

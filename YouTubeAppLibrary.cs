
using Google;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace YouTubeAppLibrary
{
    public class YouTubeLibrary
    {
        public async Task<List<Channel>> GetChannelsAsync(YouTubeService youtubeService)
        {
            ChannelsResource.ListRequest request = youtubeService.Channels.List("snippet,contentDetails,brandingSettings");
            request.Mine = true;
            ChannelListResponse response = await request.ExecuteAsync();
            List<Channel> channels = new List<Channel>();
            while (response.Items.Count > 0)
            {
                channels.AddRange(response.Items);
                if (response.NextPageToken is not null)
                {
                    request.PageToken = response.NextPageToken;
                    response = await request.ExecuteAsync();
                }
                else
                {
                    break;
                }
            }
            return channels;
        }
        public async Task<List<Subscription>> GetSubscriptions(YouTubeService youtubeService)
        {
            string[] subscriptionRequestPart = { "snippet,contentDetails" };
            SubscriptionsResource.ListRequest request = youtubeService.Subscriptions.List(subscriptionRequestPart);
            request.Mine = true;
            request.MaxResults = 25;
            SubscriptionListResponse response = await request.ExecuteAsync();
            List<Subscription> subscriptions = new List<Subscription>();
            while (response.Items.Count > 0)
            {
                subscriptions.AddRange(response.Items);
                if (response.NextPageToken is not null)
                {
                    request.PageToken = response.NextPageToken;
                    response = await request.ExecuteAsync();
                }
                else
                {
                    break;
                }
            }
            return subscriptions;
        }
        public async Task<List<VideoCategory>> GetVideoCategoriesAsync(
            YouTubeService youtubeService
            , string countryCode
            )
        {
            VideoCategoriesResource.ListRequest request = youtubeService.VideoCategories.List("snippet");
            request.RegionCode = countryCode;
            VideoCategoryListResponse response = await request.ExecuteAsync();
            List<VideoCategory> categories = new List<VideoCategory>(response.Items);
            return categories;
        }
        public async Task<Subscription> AddSubscriptionAsync(YouTubeService youtubeService, string channelId)
        {
            Console.WriteLine("Adding subscription: channelId[{0}]", channelId);
            try
            {
                Subscription subscription = new Subscription()
                {
                    Snippet = new SubscriptionSnippet()
                    {
                        ResourceId = new ResourceId()
                        {
                            ChannelId = channelId,
                            Kind = "youtube#channel"
                        }
                    }
                };

                //SubscriptionSnippet snippet = new SubscriptionSnippet();
                //snippet.ResourceId = new ResourceId()
                //    {
                //        ChannelId = channelId,
                //        Kind = "youtube#channel"
                //    };
                //subscription.Snippet = snippet;

                SubscriptionsResource.InsertRequest request = youtubeService.Subscriptions.Insert(subscription, "snippet");
                Subscription response = await request.ExecuteAsync();
                return response;
            }
            catch (GoogleApiException ex)
            {
                if (ex.HttpStatusCode == HttpStatusCode.NotFound || ex.HttpStatusCode == HttpStatusCode.BadRequest)
                {
                    return null;
                }
                throw ex;
            }
        }
        public async Task<string> DeleteSubscriptionAsync(YouTubeService youtubeService, string subscriptionId)
        {
            Console.WriteLine("Deleting subscription: subscriptionId[{0}]", subscriptionId);
            try
            {
                SubscriptionsResource.DeleteRequest request = youtubeService.Subscriptions.Delete(subscriptionId);
                string response = await request.ExecuteAsync();
                return response;
            }
            catch (GoogleApiException ex)
            {
                if (ex.HttpStatusCode == HttpStatusCode.NotFound || ex.HttpStatusCode == HttpStatusCode.BadRequest)
                {
                    return null;
                }
                throw ex;
            }
        }
        public async Task ShowPlaylists(YouTubeService youtubeService)
        {
            try
            {
                List<Playlist> playlists = await GetPlaylistsAsync(youtubeService);
                foreach (var item in playlists)
                {
                    Console.WriteLine("PLAYLIST: id[{0}] title[{1}]", item.Id, item.Snippet.Title);
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            finally
            {
                Console.WriteLine("main is done");
            }
        }
        public async Task<List<Playlist>> GetPlaylistsAsync(YouTubeService youtubeService)
        {
            List<Playlist> playlists = new List<Playlist>();

            // Define and execute the API List request
            PlaylistsResource.ListRequest request = youtubeService.Playlists.List("snippet,contentDetails");
            request.Mine = true;
            request.MaxResults = 25L;
            PlaylistListResponse response = await request.ExecuteAsync();
            while (response.Items.Count > 0)
            {
                playlists.AddRange(response.Items);
                if (response.NextPageToken is not null)
                {
                    request.PageToken = response.NextPageToken;
                    response = await request.ExecuteAsync();
                }
                else
                {
                    break;
                }
            }
            return playlists;
        }
        public async Task<List<PlaylistItem>> GetPlaylistItemsAsync(YouTubeService youtubeService, string playlistId)
        {
            PlaylistItemsResource.ListRequest request = new PlaylistItemsResource.ListRequest(youtubeService, "snippet,contentDetails");
            request.PlaylistId = playlistId;
            request.MaxResults = 25;
            List<PlaylistItem> playlist = new List<PlaylistItem>();
            string nextPageToken = "";
            while (nextPageToken != null)
            {
                var response = await request.ExecuteAsync();
                if (response.Items != null)
                {
                    playlist.AddRange(response.Items);
                    if (response.NextPageToken != null)
                    {
                        request.PageToken = response.NextPageToken;
                        response = await request.ExecuteAsync();
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return playlist;
        }
        public async Task<Playlist> AddPlaylistAsync(
            YouTubeService youtubeService,
            string playlistTitle,
            string playlistDescription
            )
        {
            Console.WriteLine("Adding playlist: title[{0}] desc[{1}]", playlistTitle, playlistDescription);
            try
            {
                // Define the Playlist object, which will be uploaded as the request body.
                Playlist playlist = new Playlist();

                // Add the snippet object property to the Playlist object.
                PlaylistSnippet snippet = new PlaylistSnippet();
                snippet.DefaultLanguage = "en";
                snippet.Description = playlistDescription;
                string[] tags = {
                    "sample playlist",
                    "API call",
                };
                snippet.Tags = tags;
                snippet.Title = playlistTitle;
                playlist.Snippet = snippet;

                // Add the status object property to the Playlist object.
                PlaylistStatus status = new PlaylistStatus();
                status.PrivacyStatus = "private";
                playlist.Status = status;

                // Define and execute the API request
                PlaylistsResource.InsertRequest request = youtubeService.Playlists.Insert(playlist, "snippet,status");
                Playlist response = await request.ExecuteAsync();
                return response;
            }
            catch (GoogleApiException ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
        public string TypeDetailsToString(Type t)
        {
            return
                "Name[" + t.Name
                + "],FullName[" + t.FullName
                + "],IsClass[" + (t.IsClass ? "true" : "false")
                + "],AssemblyQualifiedName[" + t.AssemblyQualifiedName
                + "],Namespace[" + t.Namespace
                + "]"
                ;
        }
        public string GoogleExceptionToString(Google.GoogleApiException ex)
        {
            return
                "Error.Code[" + ex.Error.Code
                + "]\n\tError.Message[" + ex.Error.Message
                + "]\n\tServiceName[" + ex.ServiceName
                + "]\n\tSource[" + ex.Source
                + "]\n\tError.ToString[" + ex.Error.ToString()
                + "]\n"
                ;
        }
        public async Task<string> DeletePlaylistAsync(
            YouTubeService youtubeService,
            string playlistId
            )
        {
            Console.WriteLine("Deleting playlist: id=[{0}]", playlistId);
            try
            {
                PlaylistsResource.DeleteRequest request =
                    new PlaylistsResource(youtubeService).Delete(playlistId);
                //PlaylistsResource.DeleteRequest request =
                //    new PlaylistsResource.DeleteRequest(youtubeService, playlistId);

                string response = await request.ExecuteAsync();
                return response;
            }
            catch (GoogleApiException ex)
            {
                if (ex.HttpStatusCode != HttpStatusCode.NotFound)
                {
                    Console.WriteLine(GoogleExceptionToString(ex));
                    throw ex;
                }
                return null;
            }
        }
        public async Task<Playlist> UpdatePlaylistAsync(
            YouTubeService youtubeService,
            string playlistId,
            string playlistTitle,
            string playlistDescription
            )
        {
            Console.WriteLine("Updating playlist: id=[{0}] newTitle[{1}] newDesc[{2}]", playlistId, playlistTitle, playlistDescription);
            try
            {
                Playlist body = new Playlist()
                {
                    Id = playlistId,
                    Snippet = new PlaylistSnippet()
                    {
                        Title = playlistTitle,
                        Description = playlistDescription
                    }
                };
                PlaylistsResource.UpdateRequest request = new PlaylistsResource.UpdateRequest(youtubeService,
                    body,
                    new List<string> { "snippet" }
                    );
                Playlist response = await request.ExecuteAsync();
                return response;
            }
            catch (GoogleApiException ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
        private async Task<List<Video>> getVideoListAsync(VideosResource.ListRequest request)
        {
            List<Video> videos = new List<Video>();
            request.MaxResults = 25L;
            VideoListResponse response = await request.ExecuteAsync();
            while (response.Items.Count > 0)
            {
                videos.AddRange(response.Items);
                if (response.NextPageToken is not null)
                {
                    request.PageToken = response.NextPageToken;
                    response = await request.ExecuteAsync();
                }
                else
                {
                    break;
                }
            }
            return videos;
        }
        public async Task<List<Video>> GetVideosByIdAsync(
            YouTubeService youtubeService
            , string CommaSeparatedVideoIds
            )
        {
            List<Video> videos = new List<Video>();
            if (CommaSeparatedVideoIds.Length > 0)
            {
                VideosResource.ListRequest request = youtubeService.Videos.List("snippet");
                request.Id = CommaSeparatedVideoIds;
                videos = await getVideoListAsync(request);
            }
            return videos;
        }
        public async Task<List<Video>> GetLikedVideosAsync(YouTubeService youtubeService)
        {
            VideosResource.ListRequest request = youtubeService.Videos.List("snippet");
            request.MyRating = VideosResource.ListRequest.MyRatingEnum.Like;
            VideoListResponse response = await request.ExecuteAsync();
            List<Video> videos = await getVideoListAsync(request);
            return videos;
        }
    }
}
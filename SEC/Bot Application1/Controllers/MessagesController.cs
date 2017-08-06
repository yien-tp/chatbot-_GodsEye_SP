using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Web.Services.Description;
using System.Linq;
using System;
using System.Collections.Generic;
using BotCampDemo.Model;
using Microsoft.ProjectOxford.Vision;
using Microsoft.Cognitive.LUIS;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Vision.Contract;
using Newtonsoft.Json.Linq;
using Microsoft.ProjectOxford.Face.Contract;
using System.Threading;

namespace Bot_Application1
{
    public class Global

    {
        public static string userid;

    }
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                Activity reply = activity.CreateReply();
                //Trace.TraceInformation(JsonConvert.SerializeObject(reply, Formatting.Indented));
                
                if (activity.Attachments?.Count > 0 && activity.Attachments.First().ContentType.StartsWith("image"))
                {
                    //user傳送一張照片
                    ImageTemplate(reply, activity.Attachments.First().ContentUrl);
                    
                }
                //else if(activity.Text == "quick") //Suggestion button
                //{
                //    reply.Text = "samplemenu";
                //    reply.SuggestedActions = new SuggestedActions()
                //    {
                //        Actions = new List<CardAction>()
                //        {
                //            new CardAction(){Title = "USD",Type=ActionTypes.ImBack,Value="USD"},
                //            new CardAction(){Title = "url",Type=ActionTypes.OpenUrl,Value="www.google.com.tw"}
                //            //new CardAction(){Title = "location",Type=ActionTypes.OpenUrl,Value=""}
                //        }
                //    };
                //}
                else
                {
                    if (activity.ChannelId == "facebook")
                    {
                        string nametest = activity.Text;
                        bool keyin = nametest.StartsWith("名稱");
                        
                        var fbData = JsonConvert.DeserializeObject<FBChannelModel>(activity.ChannelData.ToString());
                        if (fbData.postback != null)
                        {
                            
                            var url = fbData.postback.payload.Split('>')[1];
                            if (fbData.postback.payload.StartsWith("Face>"))
                            {
                                //faceAPI
                                FaceServiceClient client = new FaceServiceClient("put_your_key_here", "https://westcentralus.api.cognitive.microsoft.com/face/v1.0");
                                CreatePersonResult result_Person = await client.CreatePersonAsync("security", Global.userid);
                                await client.AddPersonFaceAsync("security", result_Person.PersonId, url);
                                
                                await client.TrainPersonGroupAsync("security");
                                var result = client.GetPersonGroupTrainingStatusAsync("security");
                                reply.Text = $"使用者已創立,person_id為:{result_Person.PersonId}";
                                
                                

                                //var result = await client.DetectAsync(url, true, false);
                                //foreach (var face in result)
                                //{
                                //    var id = face.FaceId;
                                //    reply.Text = $"{id}";  
                                //}

                            }
                            //if (fbData.postback.payload.StartsWith("Analyze>"))
                            //{
                            //    //辨識圖片
                            //    VisionServiceClient client = new VisionServiceClient("88b8704fe3bd4483ac755befdc8624db", "https://westcentralus.api.cognitive.microsoft.com/vision/v1.0");
                            //    var result = await client.AnalyzeImageAsync(url, new VisualFeature[] { VisualFeature.Description });
                            //    reply.Text = result.Description.Captions.First().Text;
                            //}
                            else
                                reply.Text = $"nope";
                        }
                        else if (keyin == true)
                        {
                            Global.userid = activity.Text.Trim("名稱".ToCharArray());
                            reply.Text = $"name set as:{Global.userid}";
                        }
                        else
                        {
                            reply.Text = $"nope";
                        }
                            
                    }
                }
                await connector.Conversations.ReplyToActivityAsync(reply);
            }
                       
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        //private async Task <string> ProcessLUIS(string text)
        //{
        //    using (LuisClient client = new LuisClient("48d2dd1c-c0e4-418b-abb9-fab10b31e5ba", "7b780ccf7f9044a2a0cfd26affd6f13b"))
        //    {
        //        var result = await client.Predict(text);
        //        if(result.Intents.Count() <= 0 || result.TopScoringIntent.Name != "查匯率")
        //        {
        //            return "看不懂";
        //        }

        //        if(result.Entities == null || !result.Entities.Any(x=>x.Key.StartsWith("幣別")))
        //        {
        //            return "目前只支援日幣與美金QQ";
        //        }
        //        var currency = result.Entities?.Where(x => x.Key.StartsWith("幣別"))?.First().Value[0].Value;
        //        return $"查詢的外幣是{currency},價格是xxx";
        //    }

        //}

        private void ImageTemplate(Activity reply, string url)
        {
            List<Attachment> att = new List<Attachment>();
            att.Add(new HeroCard()
            {
                Title = "Cognitive services",
                Subtitle = "Select from below",
                Images = new List<CardImage>() { new CardImage(url) },
                Buttons = new List<CardAction>()
                    {
                        new CardAction(ActionTypes.PostBack, "上傳使用者圖片", value: $"Face>{url}"),
                        new CardAction(ActionTypes.PostBack, "辨識圖片", value: $"Analyze>{url}")
                    }
            }.ToAttachment());

            reply.Attachments = att;
        }

       

    }
        

}
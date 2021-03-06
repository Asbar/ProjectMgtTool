using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DHTMLX.Scheduler;
using DHTMLX.Common;
using DHTMLX.Scheduler.Data;
using DHTMLX.Scheduler.Controls;

using SEP.Models;
namespace SEP.Controllers
{
    public class CalendarController : Controller
    {
        public ActionResult Index()
        {
            //Being initialized in that way, scheduler will use CalendarController.Data as a the datasource and CalendarController.Save to process changes
            var scheduler = new DHXScheduler(this);
            scheduler.Skin = DHXScheduler.Skins.Flat;
            /*
             * It's possible to use different actions of the current controller
             *      var scheduler = new DHXScheduler(this);     
             *      scheduler.DataAction = "ActionName1";
             *      scheduler.SaveAction = "ActionName2";
             * 
             * Or to specify full paths
             *      var scheduler = new DHXScheduler();
             *      scheduler.DataAction = Url.Action("Data", "Calendar");
             *      scheduler.SaveAction = Url.Action("Save", "Calendar");
             */

            /*
             * The default codebase folder is ~/Scripts/dhtmlxScheduler. It can be overriden:
             *      scheduler.Codebase = Url.Content("~/customCodebaseFolder");
             */

            scheduler.InitialDate = new DateTime();
            scheduler.Config.multi_day = true;
            scheduler.LoadData = true;
            scheduler.EnableDataprocessor = true;

            return View(scheduler);
        }

        public ContentResult Data()
        {
            var data = new SchedulerAjaxData(new CalendarDataContext().Events);
            return (ContentResult)data;
            //    return new SchedulerAjaxData(new SampleDataContext().Events);
           // return (ContentResult)data;
        }

        public ContentResult Save(int? id, FormCollection actionValues)
        {
           
            var action = new DataAction(actionValues);
          //  DHXSchedulerDataItem data = new DHXSchedulerDataItem();
         //   CalendarDataContext data = new CalendarDataContext();
            var changedEvent = (Event)DHXEventsHelper.Bind(typeof(Event), actionValues);
            
            var data = new CalendarDataContext();
            try
            {                
                switch (action.Type)
                {
                    case DataActionTypes.Insert:
                        //do insert

                        //action.TargetId = changedEvent.id;//assign postoperational id
                        changedEvent.LecturerId = "IT14121548";           
                        data.Events.InsertOnSubmit(changedEvent);
                        break;
                    case DataActionTypes.Delete:
                        //do delete
                        changedEvent = data.Events.SingleOrDefault(ev => ev.id == action.SourceId);
                        data.Events.DeleteOnSubmit(changedEvent);
                        break;
                    default:// "update"                          
                            //do update
                        var eventToUpdate = data.Events.SingleOrDefault(ev => ev.id == action.SourceId);
                        DHXEventsHelper.Update(eventToUpdate, changedEvent, new List<string>() { "id" });//update all properties, except for id
                        break;
                }
                                data.SubmitChanges();
                action.TargetId = changedEvent.id;
            }
            catch(Exception e)
            {
                action.Type = DataActionTypes.Error;
            }
            return (ContentResult)new AjaxSaveResponse(action);
        }
    }
}


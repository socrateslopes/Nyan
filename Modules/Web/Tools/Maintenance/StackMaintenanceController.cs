﻿using System.Web.Http;
using Nyan.Core.Modules.Maintenance;

namespace Nyan.Modules.Web.Tools.Maintenance
{
    [RoutePrefix("stack/tools/maintenance")]
    public class StackMaintenanceController : ApiController
    {
        [Route("run")]
        [HttpGet]
        public IMaintenanceEventEntry DoMaintenance()
        {
            return Factory.DoMaintenance();
        }

        [Route("run/force")]
        [HttpGet]
        public IMaintenanceEventEntry DoMaintenanceFull()
        {
            return Factory.DoMaintenance(true);
        }

        [Route("task/list")]
        [HttpGet]
        public object ListMaintenanceTasks()
        {
            return Instances.Schedule;
        }
    }
}
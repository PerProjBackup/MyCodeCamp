﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCodeCamp.Controllers
{
  public abstract class BaseController : Controller
  {
    public const string URLHELPER = "URLHELPER";

    public override void OnActionExecuting(ActionExecutingContext context)
    {
      base.OnActionExecuting(context);
      context.HttpContext.Items[URLHELPER] = Url;
    }
  }
}
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using WeddingPlanner.Models;

namespace WeddingPlanner.Controllers;

public class SessionCheckAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // Find the session, but remember it may be null so we need int?
        int? userId = context.HttpContext.Session.GetInt32("UserId");
        // Check to see if we got back null
        if(userId == null)
        {
            // Redirect to the Index page if there was nothing in session
            // "Home" here is referring to "HomeController", you can use any controller that is appropriate here
            context.Result = new RedirectToActionResult("SignIn", "Home", null);
        }
    }
}


public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
     private MyContext _context;  

    public HomeController(ILogger<HomeController> logger,MyContext context)
    {
        _logger = logger;
        _context = context;  
    }
    [SessionCheck]
    public IActionResult Index()
    {
        ViewBag.userId = HttpContext.Session.GetInt32("UserId");
        ViewBag.dasmat = _context.Weddings.Include(e=> e.ftesat).ToList();
        return View();
    }
    [HttpGet("SignIn")]
    public IActionResult SignIn(){
        
        return View("Auth");
    }
   
     [HttpPost("Create")]
    public IActionResult CreateUser(User userFromView){
        if (ModelState.IsValid)
        {
            PasswordHasher<User> Hasher = new PasswordHasher<User>();   
            // Updating our newUser's password to a hashed version         
            userFromView.Password = Hasher.HashPassword(userFromView, userFromView.Password);
         _context.Add(userFromView);
         _context.SaveChanges();
         return RedirectToAction("SignIn");   
        }
        return View("Auth");
    }
        [HttpPost("Login")]
    public IActionResult Login(SignIn userSubmission){

        if (ModelState.IsValid)
        {
            User? userInDb = _context.Users.FirstOrDefault(u => u.Email == userSubmission.SEmail);        
        // If no user ex
        if (userInDb == null)
        {   
            ModelState.AddModelError("Email", "Invalid Email");            
            return View("Auth"); 
        }
        PasswordHasher<SignIn> hasher = new PasswordHasher<SignIn>();                    
        // Verify provided password against hash stored in db        
        var result = hasher.VerifyHashedPassword(userSubmission, userInDb.Password, userSubmission.SPassword);   
         if(result == 0)        
        {            
             ModelState.AddModelError("Password", "Invalid Password");            
            return View("Auth");       
        }
        int UserId = userInDb.UserId;
        HttpContext.Session.SetInt32("UserId", userInDb.UserId);
        return RedirectToAction("Index");

            
        }
        

        return View("Auth"); 
    }

      [HttpGet("LogOut")]
    public IActionResult LogOut(){
        HttpContext.Session.Clear();
        return RedirectToAction("SignIn");
    }
     [SessionCheck]

    [HttpGet("AddWedding")]
    public IActionResult AddWedding(){
        return View();
    }
     [SessionCheck]
    [HttpPost("CreateWedding")]
    public IActionResult CreateWedding(Wedding postiNgaView){
        int? userId = HttpContext.Session.GetInt32("UserId");
        if (ModelState.IsValid)
        {

            postiNgaView.UserId = userId;
            _context.Add(postiNgaView);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        return View("Index");
    }
     [SessionCheck]
        [HttpGet("Shko")]
    public IActionResult Shko(int id){
        // Post posti = _context.Posts.FirstOrDefault(e=>e.PostId
        Ftesa newLike = new Ftesa();
        newLike.WeddingId = id;
        newLike.UserId = HttpContext.Session.GetInt32("UserId");
        _context.Add(newLike);
        _context.SaveChanges();

        return RedirectToAction("Index");
    }
     [SessionCheck]
    [HttpGet("ik")]
    public IActionResult Ik(int id){
        // Post posti = _context.Posts.FirstOrDefault(e=>e.PostId
        int? UserId = HttpContext.Session.GetInt32("UserId");
        Ftesa likengaDb = _context.Ftesat.FirstOrDefault(e=> e.WeddingId== id && e.UserId== UserId);
        _context.Remove(likengaDb);
    
        _context.SaveChanges();

        return RedirectToAction("Index");
    }
     [SessionCheck]
    [HttpGet("DeleteDasma")]
    public IActionResult DeleteDasma(int id){
        // Post posti = _context.Posts.FirstOrDefault(e=>e.PostId
        Wedding likengaDb = _context.Weddings.FirstOrDefault(e=> e.WeddingId== id);
        _context.Remove(likengaDb);
    
        _context.SaveChanges();

        return RedirectToAction("Index");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

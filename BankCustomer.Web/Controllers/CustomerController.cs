using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Drawing.Printing;
using System.Text;
using System.Text.Json.Nodes;

namespace BankCustomer.Web.Controllers
{
    public class CustomerController : Controller
    {
        private IMemoryCache _cache;
        public CustomerController(IMemoryCache memoryCache) {
            _cache = memoryCache;
        }

        private List<Customer> DressList(List<Customer> obj)
        {
            List<Customer> result = new List<Customer>();
            foreach (var item in obj)
            {
                item.FullName = item.first_name + ", " + item.last_name;
                item.age = CalculateAge(item.date_birth);
                result.Add(item);
            }
            return result.OrderByDescending(x => x.join_date).ToList();
        }

        private Customer DressCustomer(Customer obj)
        {
            
                obj.FullName = obj.first_name + ", " + obj.last_name;
                obj.age = CalculateAge(obj.date_birth);
            
            
            return obj;
        }


        // GET: CustomerController
        public ActionResult Index()
        {

            List<Customer> model = _cache.Get<List<Customer>>("CustomerList");
            string modeljson = _cache.Get<string>("CustomerListJson");
            List<Customer> jsonModel = new List<Customer>();
            if (model == null)
            {
                
                // Demo API that produces a json that you will store into the session.
                string apiUrl = "https://my.api.mockaroo.com/customers.json?key=03c46990";

                // Use HttpClient to make a GET request to the API and retrieve the JSON data
                using (var httpClient = new HttpClient())
                {
                    HttpResponseMessage response = httpClient.GetAsync(apiUrl).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        string jsonContent = response.Content.ReadAsStringAsync().Result;
                        _cache.Set("CustomerListJson", jsonContent, TimeSpan.FromMinutes(30)); // Cache for 30 minutes // for debugging purposes
                        // Deserialize the JSON data into a list of Customer objects
                        jsonModel = JsonConvert.DeserializeObject<List<Customer>>(jsonContent);
                        
                        jsonModel = DressList(jsonModel);
                    }
                    else
                    {
                        // Handle the API error here
                        // You can return an error view or take appropriate action
                        return RedirectToAction("Error", "Home");
                    }
                }

                //couldn't be loaded from memory, lets load it
                _cache.Set("CustomerList", jsonModel, TimeSpan.FromMinutes(30)); // Cache for 30 minutes
                // Pass the customers list as the model to the view
                return View(jsonModel);
            }
            else
            {
                return View(model);
            }

        }

        // GET: CustomerController/Details/5
        public ActionResult Details(int id)
        {
            //load session data from cache.
            List<Customer> model = _cache.Get<List<Customer>>("CustomerList");
            string modeljson = _cache.Get<string>("CustomerListJson");

            //Check if session is still valid.
            if (string.IsNullOrEmpty(modeljson))
            {
                //error state, redirect to error page.
                return RedirectToAction("Error", "Home");
            }

            Customer singlemodel = model.FirstOrDefault(x => x.customer_number == id);

            if (singlemodel == null)
            {
                //Error state - can't have a customer without a customer number.
                return RedirectToAction("Error", "Home");
            }

            return View(singlemodel);
        }

        // GET: CustomerController/Create
        public ActionResult Create()
        {
            //load session data from cache.
            List<Customer> model = _cache.Get<List<Customer>>("CustomerList");
            string modeljson = _cache.Get<string>("CustomerListJson");

            //Check if session is still valid.
            if (string.IsNullOrEmpty(modeljson))
            {
                //error state, redirect to error page.
                return RedirectToAction("Error", "Home");
            }

            return View();
        }

        // POST: CustomerController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Customer newCustomer)
        {
            try
            {
                //load session data from cache.
                List<Customer> model = _cache.Get<List<Customer>>("CustomerList");
                string modeljson = _cache.Get<string>("CustomerListJson");

                //Check if session is still valid.
                if (string.IsNullOrEmpty(modeljson))
                {
                    //error state, redirect to error page.
                    return RedirectToAction("Error", "Home");
                }
                //Get max customer id number - assume since no db connection next int is new customer number. This is ONLY to not disrupt unique identifiers, assuming customer number is the unique identifier from this data set.
                // not valid in real world scenario!!!

                int maxCustNumberPlusOne = model.Max(x => x.customer_number) + 1;

                newCustomer.customer_number = maxCustNumberPlusOne;

                //model validation now.

                if (string.IsNullOrEmpty(newCustomer.first_name))
                {
                    ModelState.AddModelError("first_name", "first name is required.");
                    return View(newCustomer);
                }
                if (string.IsNullOrEmpty(newCustomer.last_name))
                {
                    ModelState.AddModelError("last_name", "last name is required.");
                    return View(newCustomer);
                }
                if (newCustomer.date_birth == null || newCustomer.date_birth > DateTime.Now )
                {
                    ModelState.AddModelError("date_birth", "DOB is required.");
                    return View(newCustomer);
                }
                if (string.IsNullOrEmpty(newCustomer.ssn))
                {
                    ModelState.AddModelError("ssn", "SSN is required.");
                    return View(newCustomer);
                }

                if (string.IsNullOrEmpty(newCustomer.mobile_phone_number))
                {
                    ModelState.AddModelError("mobile_phone_number", "mobile is required.");
                    return View(newCustomer);
                }

                if (string.IsNullOrEmpty(newCustomer.primary_address.address_line_1) 
                    
                    )
                {
                    ModelState.AddModelError("address_line_1", "All Address fields are required");
                    return View(newCustomer);
                }

                if (string.IsNullOrEmpty(newCustomer.primary_address.city)
                    
                    )
                {
                    ModelState.AddModelError("city", "All Address fields are required");
                    return View(newCustomer);
                }

                if (string.IsNullOrEmpty(newCustomer.primary_address.state)
                    
                    )
                {
                    ModelState.AddModelError("state", "All Address fields are required");
                    return View(newCustomer);
                }

                if (string.IsNullOrEmpty(newCustomer.primary_address.zip_code)
                    )
                {
                    ModelState.AddModelError("zip_code", "All Address fields are required");
                    return View(newCustomer);
                }
                else
                {
                    if (newCustomer.primary_address.zip_code.Length !=5
                    )
                    {
                        ModelState.AddModelError("zip_code", "Zipcodes should be in a 5 digit format.");
                        return View(newCustomer);
                    }
                }

                //valid at this point attempt to post back to url
                try
                {
                    // Serialize the customer object to JSON
                    string json = JsonConvert.SerializeObject(newCustomer);

                    // Set up HttpClient
                    using (var httpClient = new HttpClient())
                    {
                        // Set the API endpoint URL
                        string apiUrl = "https://my.api.mockaroo.com/customers.json?key=03c46990"; 

                        // Create a StringContent with the JSON data
                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        // Send the POST request to the API
                        HttpResponseMessage response = await httpClient.PostAsync(apiUrl, content);

                        // Check if the request was successful
                        if (response.IsSuccessStatusCode)
                        {
                            // Handle a successful response (e.g., redirect or return a success message)

                            model.Add(newCustomer);
                            //couldn't be loaded from memory, lets load it
                            _cache.Set("CustomerList", model, TimeSpan.FromMinutes(30)); // Cache for 30 minutes

                            return RedirectToAction(nameof(Index));
                        }
                        else
                        {
                            // Handle an unsuccessful response (e.g., show an error message)
                            // instructions say no failure possible.
                            ViewBag.ErrorMessage = "Failed to create a customer.";
                            return View();
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle any exceptions that occur during the process
                    ViewBag.ErrorMessage = "An error occurred: " + ex.Message;
                    return View();
                }


            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the process
                ViewBag.ErrorMessage = "An error occurred: " + ex.Message;
                return View();
            }
        }

        // GET: CustomerController/Edit/5
        public ActionResult Edit(int id)
        {
            //load session data from cache.
            List<Customer> model = _cache.Get<List<Customer>>("CustomerList");
            string modeljson = _cache.Get<string>("CustomerListJson");

            //Check if session is still valid.
            if (string.IsNullOrEmpty(modeljson))
            {
                //error state, redirect to error page.
                return RedirectToAction("Error", "Home");
            }

            Customer singlemodel = model.FirstOrDefault(x => x.customer_number == id);

            if (singlemodel == null)
            {
                //Error state - can't have a customer without a customer number.
                return RedirectToAction("Error", "Home");
            }

            return View(singlemodel);
        }

        // POST: CustomerController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Customer newCustomer)
        {
            try
            {
                //load session data from cache.
                List<Customer> model = _cache.Get<List<Customer>>("CustomerList");
                string modeljson = _cache.Get<string>("CustomerListJson");

                //Check if session is still valid.
                if (string.IsNullOrEmpty(modeljson))
                {
                    //error state, redirect to error page.
                    return RedirectToAction("Error", "Home");
                }
                //Get max customer id number - assume since no db connection next int is new customer number. This is ONLY to not disrupt unique identifiers, assuming customer number is the unique identifier from this data set.
                // not valid in real world scenario!!!

                int maxCustNumberPlusOne = model.Max(x => x.customer_number) + 1;
                if (newCustomer.customer_number == null || newCustomer.customer_number == 0)
                {
                    newCustomer.customer_number = maxCustNumberPlusOne;
                }
                

                //model validation now.

                if (string.IsNullOrEmpty(newCustomer.first_name))
                {
                    ModelState.AddModelError("first_name", "first name is required.");
                    return View(newCustomer);
                }
                if (string.IsNullOrEmpty(newCustomer.last_name))
                {
                    ModelState.AddModelError("last_name", "last name is required.");
                    return View(newCustomer);
                }
                if (newCustomer.date_birth == null || newCustomer.date_birth > DateTime.Now)
                {
                    ModelState.AddModelError("date_birth", "DOB is required.");
                    return View(newCustomer);
                }
                if (string.IsNullOrEmpty(newCustomer.ssn))
                {
                    ModelState.AddModelError("ssn", "SSN is required.");
                    return View(newCustomer);
                }

                if (string.IsNullOrEmpty(newCustomer.mobile_phone_number))
                {
                    ModelState.AddModelError("mobile_phone_number", "mobile is required.");
                    return View(newCustomer);
                }

                if (string.IsNullOrEmpty(newCustomer.primary_address.address_line_1)

                    )
                {
                    ModelState.AddModelError("address_line_1", "All Address fields are required");
                    return View(newCustomer);
                }

                if (string.IsNullOrEmpty(newCustomer.primary_address.city)

                    )
                {
                    ModelState.AddModelError("city", "All Address fields are required");
                    return View(newCustomer);
                }

                if (string.IsNullOrEmpty(newCustomer.primary_address.state)

                    )
                {
                    ModelState.AddModelError("state", "All Address fields are required");
                    return View(newCustomer);
                }

                if (string.IsNullOrEmpty(newCustomer.primary_address.zip_code)
                    )
                {
                    ModelState.AddModelError("zip_code", "All Address fields are required");
                    return View(newCustomer);
                }

                //valid at this point attempt to post back to url
                try
                {
                    // Serialize the customer object to JSON
                    string json = JsonConvert.SerializeObject(newCustomer);

                    // Set up HttpClient
                    using (var httpClient = new HttpClient())
                    {
                        // Set the API endpoint URL
                        string apiUrl = "https://my.api.mockaroo.com/customers.json?key=03c46990";

                        // Create a StringContent with the JSON data
                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        // Send the POST request to the API
                        HttpResponseMessage response = await httpClient.PostAsync(apiUrl, content);

                        // Check if the request was successful
                        if (response.IsSuccessStatusCode)
                        {
                            // Handle a successful response (e.g., redirect or return a success message)
                            int custid = newCustomer.customer_number;
                            Customer c = model.FirstOrDefault(x => x.customer_number == custid);
                            model.Remove(c);
                            model.Add(newCustomer);
                            //couldn't be loaded from memory, lets load it
                            _cache.Set("CustomerList", model, TimeSpan.FromMinutes(30)); // Cache for 30 minutes

                            return RedirectToAction(nameof(Index));
                        }
                        else
                        {
                            // Handle an unsuccessful response (e.g., show an error message)
                            // instructions say no failure possible.
                            ViewBag.ErrorMessage = "Failed to create a customer.";
                            return View();
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle any exceptions that occur during the process
                    ViewBag.ErrorMessage = "An error occurred: " + ex.Message;
                    return View();
                }


            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the process
                ViewBag.ErrorMessage = "An error occurred: " + ex.Message;
                return View();
            }
        }

        // GET: CustomerController/Delete/5
        public ActionResult Delete(int id)
        {
            //load session data from cache.
            List<Customer> model = _cache.Get<List<Customer>>("CustomerList");
            string modeljson = _cache.Get<string>("CustomerListJson");

            //Check if session is still valid.
            if (string.IsNullOrEmpty(modeljson))
            {
                //error state, redirect to error page.
                return RedirectToAction("Error", "Home");
            }

            Customer singlemodel = model.FirstOrDefault(x => x.customer_number == id);

            if (singlemodel == null)
            {
                //Error state - can't have a customer without a customer number.
                return RedirectToAction("Error", "Home");
            }

            return View(singlemodel);
        }

        // POST: CustomerController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(Customer cust)
        {
            try
            {
                //load session data from cache.
                List<Customer> model = _cache.Get<List<Customer>>("CustomerList");
                string modeljson = _cache.Get<string>("CustomerListJson");

                //Check if session is still valid.
                if (string.IsNullOrEmpty(modeljson))
                {
                    //error state, redirect to error page.
                    return RedirectToAction("Error", "Home");
                }
                
                Customer c = model.FirstOrDefault(x => x.customer_number == cust.customer_number);
                model.Remove(c);
                _cache.Set("CustomerList", model, TimeSpan.FromMinutes(30)); // Cache for 30 minutes
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                //load session data from cache.
                List<Customer> model = _cache.Get<List<Customer>>("CustomerList");
                string modeljson = _cache.Get<string>("CustomerListJson");

                //Check if session is still valid.
                if (string.IsNullOrEmpty(modeljson))
                {
                    //error state, redirect to error page.
                    return RedirectToAction("Error", "Home");
                }

                Customer singlemodel = model.FirstOrDefault(x => x.customer_number == cust.customer_number);

                if (singlemodel == null)
                {
                    //Error state - can't have a customer without a customer number.
                    return RedirectToAction("Error", "Home");
                }

                return View(singlemodel);
            }
        }

        public int CalculateAge(DateTime dateOfBirth)
        {
            DateTime currentDate = DateTime.Now;
            int age = currentDate.Year - dateOfBirth.Year;

            // Adjust age if the birth date hasn't occurred yet this year
            if (dateOfBirth > currentDate.AddYears(-age))
            {
                age--;
            }

            return age;
        }

    }
}

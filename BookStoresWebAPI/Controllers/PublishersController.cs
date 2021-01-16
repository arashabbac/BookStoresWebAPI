using Microsoft.EntityFrameworkCore;
using System.Linq;
namespace BookStoresWebAPI.Controllers
{
    [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
    [Microsoft.AspNetCore.Mvc.ApiController]
    public class PublishersController : Microsoft.AspNetCore.Mvc.ControllerBase
    {
        private readonly Models.BookStoresDBContext _context;

        public PublishersController(Models.BookStoresDBContext context)
        {
            _context = context;
        }

        // GET: api/Publishers
        [Microsoft.AspNetCore.Mvc.HttpGet]
        public async System.Threading.Tasks.Task
            <Microsoft.AspNetCore.Mvc.ActionResult<System.Collections.Generic.IEnumerable<Models.Publisher>>>
            GetPublishers()
        {
            return await _context.Publishers.ToListAsync();
        }

        // GET: api/Publishers/5
        [Microsoft.AspNetCore.Mvc.HttpGet("GetPublisherDetails/{id}")]
        public async System.Threading.Tasks.Task
            <Microsoft.AspNetCore.Mvc.ActionResult<Models.Publisher>> GetPublisherDetails(int id)
        {
            //Eager Loading
            //var publisher = 
            //     _context.Publishers
            //    .Include(pub => pub.Books)
            //        .ThenInclude(book => book.Sales)
            //        .ThenInclude(store => store.Store)
            //    .Include(pub => pub.Users)
            //        .ThenInclude(user => user.Job)
            //    .Where(pub => pub.PubId == id)
            //    .FirstOrDefault();

            //Explicit Loading
            var publisher = 
                await _context.Publishers.SingleAsync(publisher => publisher.PubId == id);

            _context.Entry(publisher)
                .Collection(pub => pub.Users)
                .Query()
                .Where(user => user.LastName.Contains("karin"))
                .Load();

            _context.Entry(publisher)
                .Collection(pub => pub.Books)
                .Query()
                .Include(book => book.Sales)
                .Load();

            //var user = await _context.Users.SingleAsync(current => current.UserId == 1);

            //_context.Entry(user)
            //    .Reference(usr => usr.MiddleName)
            //    .Load();

            if (publisher == null)
            {
                return NotFound();
            }

            return publisher;
        }

        // POST: api/Publishers/5
        [Microsoft.AspNetCore.Mvc.HttpGet("PostPublisherDetails/")]
        public async System.Threading.Tasks.Task
            <Microsoft.AspNetCore.Mvc.ActionResult<Models.Publisher>> PostPublisherDetails()
        {
            Models.Publisher publisher =
                new Models.Publisher();

            Models.Book book1 = new Models.Book();
            book1.Title = "Good night moon - 1";
            book1.PublishedDate = System.DateTime.Now;

            Models.Book book2 = new Models.Book();
            book2.Title = "Good night moon - 2";
            book2.PublishedDate = System.DateTime.Now;


            Models.Sale sale1 = new Models.Sale();
            sale1.Quantity = 2;
            sale1.StoreId = "8042";
            sale1.OrderNum = "XYZ";
            sale1.PayTerms = "Net 20";
            sale1.OrderDate = System.DateTime.Now;

            Models.Sale sale2 = new Models.Sale();
            sale2.Quantity = 2;
            sale2.StoreId = "7131";
            sale2.OrderNum = "XYZ";
            sale2.PayTerms = "Net 20";
            sale2.OrderDate = System.DateTime.Now;

            book1.Sales.Add(sale1);
            book2.Sales.Add(sale2);
            publisher.Books.Add(book1);
            publisher.Books.Add(book2);

            publisher.PublisherName = "Harper & Brothers";
            publisher.City = "New York City";
            publisher.State = "NY";
            publisher.Country = "USA";

            _context.Publishers.Add(publisher);
            _context.SaveChanges();

            var publishers = _context.Publishers
                                    .Include(pub => pub.Books)
                                        .ThenInclude(book => book.Sales)
                                        .ThenInclude(store => store.Store)
                                    .Include(pub => pub.Users)
                                    .Where(pub => pub.PubId == publisher.PubId)
                                    .FirstOrDefault();

            if (publishers == null)
            {
                return NotFound();
            }

            return publishers;
        }

        // GET: api/Publishers/5
        [Microsoft.AspNetCore.Mvc.HttpGet("{id}")]
        public async System.Threading.Tasks.Task<Microsoft.AspNetCore.Mvc.ActionResult<Models.Publisher>> GetPublisher(int id)
        {
            var publisher = await _context.Publishers.FindAsync(id);

            if (publisher == null)
            {
                return NotFound();
            }

            return publisher;
        }

        // PUT: api/Publishers/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [Microsoft.AspNetCore.Mvc.HttpPut("{id}")]
        public async System.Threading.Tasks.Task<Microsoft.AspNetCore.Mvc.IActionResult> PutPublisher(int id, Models.Publisher publisher)
        {
            if (id != publisher.PubId)
            {
                return BadRequest();
            }

            _context.Entry(publisher).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PublisherExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Publishers
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [Microsoft.AspNetCore.Mvc.HttpPost]
        public async System.Threading.Tasks.Task<Microsoft.AspNetCore.Mvc.ActionResult<Models.Publisher>> PostPublisher(Models.Publisher publisher)
        {
            _context.Publishers.Add(publisher);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPublisher", new { id = publisher.PubId }, publisher);
        }

        // DELETE: api/Publishers/5
        [Microsoft.AspNetCore.Mvc.HttpDelete("{id}")]
        public async System.Threading.Tasks.Task<Microsoft.AspNetCore.Mvc.ActionResult<Models.Publisher>> DeletePublisher(int id)
        {
            var publisher = await _context.Publishers.FindAsync(id);
            if (publisher == null)
            {
                return NotFound();
            }

            _context.Publishers.Remove(publisher);
            await _context.SaveChangesAsync();

            return publisher;
        }

        private bool PublisherExists(int id)
        {
            return _context.Publishers.Any(e => e.PubId == id);
        }
    }
}

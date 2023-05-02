using BookStoreAPI.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Text;

namespace BookStoreAPI.Services;

public class BooksService
{
    private readonly IMongoCollection<Book> _booksCollection;

    // public BooksService(
    //     IOptions<BookStoreDatabaseSettings> bookStoreDatabaseSettings)
    // {
    //     var mongoClient = new MongoClient(
    //         bookStoreDatabaseSettings.Value.ConnectionString);

    //     var mongoDatabase = mongoClient.GetDatabase(
    //         bookStoreDatabaseSettings.Value.DatabaseName);

    //     _booksCollection = mongoDatabase.GetCollection<Book>(
    //         bookStoreDatabaseSettings.Value.BooksCollectionName);
    // }

    public BooksService(
            IOptions<BookStoreDatabaseSettings> bookStoreDatabaseSettings)
        {

            string connectionURL = string.Empty;
            string databaseName = string.Empty; 

            if (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("MONGO_HOST"))){
            
                Console.WriteLine("Mongo ENV Vars detected..");

                string host = GetEnvironmentVariable("MONGO_HOST", "localhost");
                Console.WriteLine("HOST: " + host);
                string port = GetEnvironmentVariable("MONGO_PORT", "27017");
                Console.WriteLine("PORT: " + port);
                string username = GetEnvironmentVariable("MONGO_USERNAME", "bookstore");
                string password = GetEnvironmentVariable("MONGO_PASSWORD", "bookstore");
                databaseName = GetEnvironmentVariable("MONGO_DATABASE", "bookstore");

                StringBuilder sb = new StringBuilder("mongodb://");
                sb.Append(username + ":" + password);
                sb.Append("@" + host + ":" + port);
                sb.Append("/" + databaseName);
                
                Console.WriteLine("Mongo Connection String: " + sb);
                connectionURL = sb.ToString();

            } else {
                Console.WriteLine("No Mongo ENV Vars detected, using default connection");
                connectionURL = bookStoreDatabaseSettings.Value.ConnectionString;
                databaseName = bookStoreDatabaseSettings.Value.DatabaseName; 
            }

            Console.WriteLine("Using connectionURL: " + connectionURL);
            Console.WriteLine("Using database name: " + databaseName);
            Console.WriteLine("Using collection name: " + bookStoreDatabaseSettings.Value.BooksCollectionName);

            var mongoClient = new MongoClient(connectionURL);
            var mongoDatabase = mongoClient.GetDatabase(databaseName);
            _booksCollection = mongoDatabase.GetCollection<Book>(
                bookStoreDatabaseSettings.Value.BooksCollectionName);
        }

    public async Task<List<Book>> GetAsync() =>
        await _booksCollection.Find(_ => true).ToListAsync();

    public async Task<Book?> GetAsync(string id) =>
        await _booksCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(Book newBook) =>
        await _booksCollection.InsertOneAsync(newBook);

    public async Task UpdateAsync(string id, Book updatedBook) =>
        await _booksCollection.ReplaceOneAsync(x => x.Id == id, updatedBook);

    public async Task RemoveAsync(string id) =>
        await _booksCollection.DeleteOneAsync(x => x.Id == id);

    public static string GetEnvironmentVariable(string name, string defaultValue)
         => Environment.GetEnvironmentVariable(name) ?? defaultValue;
}
print('START :::::: Creating Bookstore DB, Credentials and Data ::::::');

// db.auth('mongo', 'mongo');

// db = db.getSiblingDB(process.env.MONGO_INITDB_DATABASE);

db.createUser(
    {
        user: process.env.MONGO_BOOKSTORE_USER,
        pwd: process.env.MONGO_BOOKSTORE_PASSWORD,
        
        roles: [
            {
                role: "readWrite", 
                db: process.env.MONGO_INITDB_DATABASE
            }
        ]
    }
);

db.createCollection('Books');
db.Books.insertMany([
    {
        _id: ObjectId("61a6058e6c43f32854e51f51"),
        Name: "Design Patterns",
        Price: 54.93,
        Category: "Computers",
        Author: "Ralph Johnson"
    },
    {
        _id: ObjectId("61a6058e6c43f32854e51f52"),
        Name: "Clean Code",
        Price: 43.15,
        Category: "Computers",
        Author: "Robert C. Martin"
    }
]);
print('END :::::: BOOKSTORE DB, Credentials and Data successfully created!! ::::::');
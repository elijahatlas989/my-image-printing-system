# MyImage - Online Digital Photo Printing System

This is a simple ASP.NET Core MVC college project prepared for Visual Studio 2026. It follows the provided MyImage problem statement without adding unnecessary enterprise features.

## Main Features

1. Customer registration and login.
2. JPEG folder/file selection and upload.
3. Print size and quantity selection for every photo.
4. Total price calculation using prices stored in SQL Server.
5. Credit Card demo or Direct Payment selection.
6. Shipping address and purchase order number.
7. Admin purchase request list and order status update.
8. Photo folder deletion after the admin marks an order as Shipped.
9. Admin print size and price management.

## Visual Studio 2026 Setup

1. Extract the ZIP file.
2. Open `MyImagePrinting.sln` in Visual Studio 2026.
3. Wait for NuGet restore to complete.
4. In Solution Explorer, right-click `MyImagePrinting` and select **Set as Startup Project** if needed.
5. Press `Ctrl + F5` or click the green Run button.
6. SQL Server LocalDB creates `MyImageDB` automatically on the first run.

## Demo Accounts

- Register a new customer from the website.
- Admin username: `admin`
- Admin password: `Admin123`

## Exact Folder Locations

- Controllers: `Controllers`
- Database models: `Models`
- Form/page models: `ViewModels`
- Entity Framework context: `Data/ApplicationDbContext.cs`
- Razor pages: `Views`
- CSS: `wwwroot/css/site.css`
- JavaScript: `wwwroot/js/site.js`
- Uploaded JPEG files: `wwwroot/uploads/folder_XXXX`
- Optional SQL script: `SQL/MyImageDB.sql`

The user does not manually copy photos into `wwwroot/uploads`. The Upload Photos page creates the correct order folder automatically.

## Project Workflow

Register -> Login -> Upload JPEG Photos -> Select Size and Quantity -> Order Summary -> Payment -> Confirmation

## Database Note

The project uses `Database.EnsureCreated()` in `Program.cs`, so you normally do not need to run the SQL script manually. The SQL script is included for documentation and demonstration.

## Credit Card Note

The Credit Card option is only a classroom demonstration. Do not enter a real card number. The application protects the demo number with ASP.NET Core Data Protection before saving it.

## Requirement Mapping

- RS1: The browser folder/file selector accepts JPG and JPEG photos only.
- RS2: Every uploaded photo has a print size dropdown and quantity field.
- RS3: The demo card number is protected before it is stored and is unprotected for server verification.
- Purchase order number: `OrderId`, displayed as `#0001`, `#0002`, etc.
- Server folder: `folder_0001`, `folder_0002`, etc.
- Admin execution: Admin can change New -> Printing -> Shipped.
- Folder cleanup: The folder is deleted when the order is marked Shipped.

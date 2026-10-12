# Demonstration Video Script & Local Setup Guide

This guide provides instructions to run both the ASP.NET Core API and the Angular Consumer App locally, and a step-by-step script to record the demonstration video covering **GET**, **POST**, **PUT**, and **DELETE** operations.

---

## 1. Running the System Locally

### Terminal 1: Run the ASP.NET Core API
```powershell
cd C:\Users\joshu\source\repos\wp1-fall26-aws
dotnet run --project src/Wp1Fall26Aws.Api --urls "http://localhost:5180"
```
The API is now listening at `http://localhost:5180`.

### Terminal 2: Run the Angular Consumer App
```powershell
cd C:\Users\joshu\source\repos\wp1-fall26-aws\consumer-app
npm start
```
The Angular consumer application will start and serve on `http://localhost:4200`.

Open your browser to: [http://localhost:4200](http://localhost:4200)

---

## 2. Step-by-Step Video Demonstration Script

When recording your video (using OBS Studio, Windows Xbox Game Bar `Win + G`, or screen recorder), keep the browser open and (optionally) the developer tools network tab or terminal visible. The app includes a **Live API Activity Log / Telemetry** panel at the bottom of the page that displays each HTTP request, status code, duration, and endpoint live.

### Step 1: Health & GET Operation (Retrieve & Display Live Data)
1. **Show connection**: Observe the header displaying `API Connected` with a green indicator dot and the base URL `http://localhost:5180`.
2. **Demonstrate GET**: Click the **🔄 Refresh** button above the document table.
3. **Point out**:
   - The brief **Loading state** spinner appearing during the fetch.
   - The live table displaying the retrieved documents (e.g. `doc-1`, `doc-2`) with Title, File Name, Content Type, Size, Uploaded Date, and Description.
   - The **Live API Activity Log** showing `GET /documents 200 OK`.

### Step 2: POST Operation (Submit & Create Resource)
1. **Option A (File Upload)**:
   - Under **Create New Resource**, select the **📄 Upload Document** tab.
   - Click **Choose File** and select any `.txt`, `.pdf`, or image (e.g., `samples/synthetic-document.txt`).
   - Enter a title: `Demo Document` and description: `Uploaded via consumer app`.
   - Click **POST /documents (Upload)**.
2. **Option B (Quick Text Document)**:
   - Select the **✏️ Quick Text Document** tab.
   - Enter a title: `Architecture Note`.
   - Click **POST /documents (Create)**.
3. **Point out**:
   - The button shows the loading spinner during the HTTP POST.
   - A green success alert appears: *"Resource created!"*.
   - The new item appears immediately at the top of the table (via live GET refresh).
   - The Activity Log logs `POST /documents 201 Created`.

### Step 3: PUT Operation (Edit Existing Resource)
1. In the documents table, click the **✏️ Edit** button on your newly created document.
2. The **Edit Document Resource** modal dialog opens.
3. Modify the Title: `Demo Document (Updated v2)` and update the description.
4. Click **PUT /documents/{id} (Save)**.
5. **Point out**:
   - The loading state during update.
   - The green success notification.
   - The document row updates with the new title and description, and displays an `(edited)` tag.
   - The Activity Log logs `PUT /documents/{id} 200 OK`.

### Step 4: DELETE Operation (Remove Resource)
1. In the documents table, click the **🗑️ Delete** button on the document.
2. The **Confirm Removal** modal dialog appears, stating the document ID to delete.
3. Click **DELETE /documents/{id}**.
4. **Point out**:
   - The item is removed from the table.
   - The Activity Log logs `DELETE /documents/{id} 200 OK`.
5. *(Optional)* Delete all remaining items to demonstrate the **Distinct Empty State** card: *"No documents found. Use the form on the left to POST your first resource."*

### Step 5: Error Handling & Security Demonstration
1. In the top URL bar, change the port to an invalid one (e.g., `http://localhost:9999`) and click **Connect / Reload**.
2. Notice the **Clear Error Message** displayed in the red banner: *"Network connection failed. Make sure the API is running at http://localhost:9999."*
3. Change it back to `http://localhost:5180` and click **Connect / Reload** to restore connection.
4. Note that all API-provided content is rendered purely as plain text using standard Angular templating expressions, preventing any malicious HTML injection.

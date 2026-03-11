<div align="center">
  <h1>🌟 Planora - AI Study Planner</h1>
  <p>
    An intelligent, full-stack application that leverages the power of Gemini AI to generate customized daily study plans and schedules based on your subjects and topics.
  </p>

  <!-- Badges -->
  <p>
    <img src="https://img.shields.io/badge/.NET%208-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET 8" />
    <img src="https://img.shields.io/badge/Angular-DD0031?style=for-the-badge&logo=angular&logoColor=white" alt="Angular" />
    <img src="https://img.shields.io/badge/TypeScript-007ACC?style=for-the-badge&logo=typescript&logoColor=white" alt="TypeScript" />
    <img src="https://img.shields.io/badge/SQLite-003B57?style=for-the-badge&logo=sqlite&logoColor=white" alt="SQLite" />
    <img src="https://img.shields.io/badge/Gemini%20AI-8E75B2?style=for-the-badge&logo=googlebard&logoColor=white" alt="Gemini AI" />
  </p>
</div>

---

## ✨ Key Features

- 🧠 **AI-Powered Planning:** Automatically generate intelligent, comprehensive daily study plans using the Google Gemini AI API.
- 📚 **Subject & Topic Orchestration:** Full CRUD capabilities to seamlessly add, edit, track, and delete your subjects and core topics.
- 🎨 **Modern & Premium UI:** A dynamic, premium user experience featuring subtle micro-animations, glassmorphism, and a beautiful responsive dark-mode design.
- 🏗 **Clean Architecture:** A highly scalable back-end built on ASP.NET Core Clean Architecture patterns.
- ⚡ **Lightweight Database:** Uses SQLite for fast, streamlined local data storage mapping through Entity Framework Core.

## 🛠️ Architecture & Tech Stack

### Frontend (Client)
- **Framework:** Angular 17+
- **Language:** TypeScript, HTML5, Vanilla CSS
- **Design:** Custom Premium CSS with modern aesthetic principles

### Backend (API)
- **Framework:** ASP.NET Core Web API (.NET 8)
- **Language:** C#
- **ORM:** Entity Framework Core
- **Database:** SQLite (Embedded, No complex setup required)
- **AI Integration:** Google Gemini API (`Gemini-1.5-flash` configuration)

---

## 🚀 Getting Started

Follow these detailed steps to set up the project locally on your machine.

### Prerequisites

Ensure you have the following installed on your local machine:
1. **[Node.js](https://nodejs.org/)** (v18.0 or higher) & npm
2. **[Angular CLI](https://angular.io/cli)** (`npm install -g @angular/cli`)
3. **[.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)**
4. **Google Gemini API Key:** You'll need an active API key from [Google AI Studio](https://aistudio.google.com/app/apikey).

---

### Step 1: Clone the Repository

```bash
git clone https://github.com/your-username/Planora.git
cd Planora
```

---

### Step 2: Backend Setup (.NET Core API)

1. Open your terminal and navigate to the API directory:
   ```bash
   cd src/AIStudyPlanner.API
   ```

2. Establish your Environment Variables:
   Create a `.env` file in the `src/AIStudyPlanner.API` root directory. Add your Gemini API Key in the `.env` file:
   ```env
   GEMINI_API_KEY=your_gemini_api_key_here
   ```

3. Restore Dependencies and Tools:
   ```bash
   dotnet restore
   ```

4. Apply Database Migrations:
   This command creates your SQLite database automatically based on Entity Framework models.
   ```bash
   dotnet ef database update
   ```
   *(Note: If you don't have EF Core tools installed, run: `dotnet tool install --global dotnet-ef`)*

5. Run the Backend Server:
   ```bash
   dotnet run
   ```
   The API should now be running locally (e.g., `http://localhost:5000` or `https://localhost:5001`). Keep this terminal open!

---

### Step 3: Frontend Setup (Angular)

1. Open a **new terminal tab/window** and navigate to the client directory:
   ```bash
   cd client
   ```

2. Install Node Modules:
   ```bash
   npm install
   ```

3. Start the Development Server:
   ```bash
   npm start
   ```
   *(Or alternatively, `ng serve`)*

4. Open the App!
   Navigate to `http://localhost:4200` in your web browser. You can now register, log in, create subjects, and generate AI study plans.

---

## 🎯 How to Use Planora
1. **Sign Up / Log In:** Create an account to personalized your experience.
2. **Add Subjects:** Head to your dashboard and begin adding the subjects you wish to learn. Provide a title and detailed topics.
3. **Generate Plan:** Click the "Generate AI Plan" action! Planora will send your topics to the Gemini API and formulate an intelligent daily study routine.
4. **Learn & Track:** View your beautifully styled daily dashboard and begin tackling your learning topics!

---

## 🤝 Contributing

Contributions, issues, and feature requests are welcome! 
If you have ideas to make Planora better:
1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## 📄 License

This project is open-source and licensed under the **MIT License**.

<div align="center">
  <p>Made with ❤️ by an ambitious developer aiming to revolutionize studying.</p>
</div>

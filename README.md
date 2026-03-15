<div align="center">
  <img src="https://raw.githubusercontent.com/Tarikul-Islam-Anik/Animated-Fluent-Emojis/master/Emojis/Objects/Books.png" alt="Books" width="120" height="120" />
  <h1>🌟 Planora - AI Study Planner</h1>
  <p>
    <b>An intelligent, full-stack orchestration platform that leverages Generative AI to transform chaotic study schedules into structured success.</b>
  </p>

  <p>
    <img src="https://img.shields.io/badge/.NET%208-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET 8" />
    <img src="https://img.shields.io/badge/Angular%2017-DD0031?style=for-the-badge&logo=angular&logoColor=white" alt="Angular" />
    <img src="https://img.shields.io/badge/SQLite-003B57?style=for-the-badge&logo=sqlite&logoColor=white" alt="SQLite" />
    <img src="https://img.shields.io/badge/Gemini%20AI-8E75B2?style=for-the-badge&logo=googlebard&logoColor=white" alt="Gemini AI" />
    <img src="https://img.shields.io/badge/Groq-f55036?style=for-the-badge&logo=ai&logoColor=white" alt="Groq AI" />
  </p>
</div>

---

## 🚀 Overview

**Planora** is a state-of-the-art AI Study Planner designed for modern students. It doesn't just list tasks; it uses Large Language Models to intelligently distribute study topics over time, considering subject difficulty, daily availability, and exam deadlines. 

With a focus on reliability and premium aesthetics, Planora ensures your learning never stops by implementing a dual-engine AI fallback system.

## ✨ Key Features

- 🧠 **AI-Powered Orchestration:** Generate comprehensive daily study plans using **Google Gemini 1.5 Flash**.
- 🛡️ **Dual-Engine Reliability:** Built-in **AI Fallback** logic. If Gemini is unavailable, the system automatically switches to **Groq (Llama 3.1)** to ensure 100% uptime.
- 📝 **AI Practice Tests:** Instantly generate relevant Multiple Choice Questions (MCQs) for any subject to validate your learning.
- 🗂️ **Subject & Topic Management:** Full CRUD capabilities with nested topic difficulty levels.
- 📊 **Progress Analytics:** Track your study growth, task completion rates, and test scores with dynamic visual graphs.
- 🎨 **Premium Glassmorphic UI:** A beautiful, dark-mode design built with **Angular 17** and custom Vanila CSS for a sleek, distraction-free experience.
- 🏗️ **Clean Architecture:** Backend developed using Enterprise-grade patterns (.NET 8 Clean Architecture) for maximum scalability.

---

## 🛠️ Tech Stack

### Frontend
- **Framework:** Angular 17
- **Language:** TypeScript
- **Styling:** Custom Modern CSS (Glassmorphism, Micro-animations)
- **State Management:** RxJS Observables

### Backend
- **Framework:** ASP.NET Core Web API 8.0
- **Database:** SQLite (file-based, zero setup)
- **ORM:** Entity Framework Core
- **Security:** JWT (JSON Web Tokens)
- **AI Integration:** 
  - **Primary:** Gemini AI (Google AI Studio)
  - **Secondary:** Groq Cloud (Llama 3.1)

---

## ⚙️ Setup & Installation

Follow these steps to get Planora running on your local machine.

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js (v18+)](https://nodejs.org/)
- [Angular CLI](https://angular.io/cli) (`npm install -g @angular/cli`)
- API Keys: 
  - [Google Gemini API Key](https://aistudio.google.com/app/apikey)
  - [Groq API Key](https://console.groq.com/keys)

---

### Step 1: Environment Configuration

The backend requires a `.env` file to communicate with AI services.

1. Navigate to the API root: `src/AIStudyPlanner.API/`
2. Create a file named `.env`
3. Add your keys as follows:

```env
GEMINI_API_KEY=your_gemini_key_here
GROQ_API_KEY=your_groq_key_here
```

### Step 2: Backend Setup (.NET)

1. Open your terminal in the root folder.
2. Navigate to the API project:
   ```bash
   cd src/AIStudyPlanner.API
   ```
3. Restore dependencies:
   ```bash
   dotnet restore
   ```
4. Update the database (creates the SQLite file):
   ```bash
   dotnet ef database update
   ```
5. Run the server:
   ```bash
   dotnet run
   ```
   *The API will be live at `https://localhost:5001` or `http://localhost:5000`.*

### Step 3: Frontend Setup (Angular)

1. Open a **new terminal** and navigate to the client folder:
   ```bash
   cd client
   ```
2. Install packages:
   ```bash
   npm install
   ```
3. Start the development server:
   ```bash
   npm start
   ```
4. Open your browser to `http://localhost:4200`.

---

## 🏗️ Architecture Insight

Planora utilizes a **Clean Architecture** approach, ensuring the core business logic is independent of external frameworks:

- **Domain:** Core entities like `Subject`, `StudyTask`, and Enums.
- **Application:** Interfaces, DTOs, and the AI Service contracts.
- **Infrastructure:** Implementation of AI services (Fallback Logic), SQLite Data Access, and Authentication.
- **API:** RESTful Controllers handling HTTP requests and JWT validation.

---

## 🤝 Project Credits
- **Primary Developer:** [Your Name/Enrollment]
- **Special Thanks:** Faculty of [Department Name]

<div align="center">
  <p>Made with ❤️ to revolutionize the way students learn.</p>
</div>

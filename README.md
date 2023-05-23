<h1 align="center">isgood</h1>

<p align="center">
    <img src="https://img.shields.io/github/license/Isotop7/isgood" alt="License" />
    <img src="https://img.shields.io/github/v/release/Isotop7/isgood" alt="Release" />
    <img src="https://sonarcloud.io/api/project_badges/measure?project=Isotop7_isgood&metric=alert_status" alt="SonarCloud" />
    <img src="https://sonarcloud.io/api/project_badges/measure?project=Isotop7_isgood&metric=security_rating" alt="Sonarcloud security rating">
    <img src="https://sonarcloud.io/api/project_badges/measure?project=Isotop7_isgood&metric=bugs" alt="Sonarcloud bugs">
    <img src="https://sonarcloud.io/api/project_badges/measure?project=Isotop7_isgood&metric=code_smells" alt="Sonarcloud code smells">
    <img src="https://img.shields.io/github/actions/workflow/status/Isotop7/isgood/dotnet.yml?branch=main" alt="CI @ master" />
</p>

<p align="center">
    ⁉️ Ever wondered if this oat milk is still fresh or when you opened that hummus? In the past you may have thrown it away. Now there is <b>isgood</b>
</p>
<p align="center">
    📚 <b>isgood</b> is a simple and intuitive application to track your bought products and their expiration date to prevent waste of food 🥗
</p>

## 🚀 Features

- 📝 Track and log your products.
- 📊 Add information from OpenFoodFacts API.
- ⏰ Get reminders if products are due to expire.
- 📱 Mobile-friendly responsive design for on-the-go usage.

## 🛠️ Technologies and Tools

- **Frontend:** Razor, HTML, CSS.
- **Backend:** C#, ASP.NET Core, Entity Framework Core, MQTT.
- **Database:** SQLite.
- **Code Quality:** SonarCloud, CodeQL.
- **Continuous Integration:** GitHub Actions.

## 📦 Installation and Usage

### 🔙 Backend

1. Clone the repository: `git clone https://github.com/Isotop7/isgood.git`
2. Navigate to the project directory: `cd isgood/isgood`
3. Build the application: `dotnet build`
4. Copy the config file `isgood.tmpl.json`, rename it to `isgood.json` and adjust it
5. Run the application: `dotnet run`
6. Open your browser and access the port shown in the console
7. Test the insertion of products with MQTT

### 💻 Client

*TODO*

## 😥 What's missing?

### 🔙 Backend

- Authentication: Add authentication on the Web UI so only you can edit and delete your digital fridge
- Customize WebUI: Extend `picocss` with own theme
- Administrative Functions: Create and Update backend users
- Personalize fridge: Assign products to users or user groups, send notifications to assigned entity
- Documentation: Provide screenshots and basic help guide
- Automated testing: Provide automated testing for better code qualitxy

### 💻 Client

- `isgood-esp`: Reference implementation of a ESP microcontroller and a connected barcode scanner to check in products

## 🤝 Contributing

Contributions are welcome! Fork the repository, make your changes, and submit a pull request.

## 📄 License

This project is licensed under the [MIT License](LICENSE).

## 👤 Author

- GitHub: [@Isotop7](https://github.com/Isotop7)
- LinkedIn: [Hendrik Röder](https://www.linkedin.com/in/hendrik-r%C3%B6der-9b8483198/)

---

⭐️ If you find this project helpful, give it a star and share it with others! ⭐️

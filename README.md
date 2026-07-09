# WorkingTimeTracking
Time tracking software for businesses
# Учёт рабочего времени (Time Tracking System)

[![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)](https://learn.microsoft.com/ru-ru/dotnet/csharp/)
[![.NET](https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white)](https://dotnet.microsoft.com/)
[![MySQL](https://img.shields.io/badge/MySQL-4479A1?style=for-the-badge&logo=mysql&logoColor=white)](https://www.mysql.com/)
[![WPF](https://img.shields.io/badge/WPF-0078D6?style=for-the-badge&logo=windows&logoColor=white)](https://learn.microsoft.com/ru-ru/dotnet/desktop/wpf/)

## 📋 Описание проекта

**"Учёт рабочего времени"** — это десктопное приложение для ведения табеля учёта рабочего времени сотрудников на предприятии. Разработано в качестве дипломного проекта.

Система позволяет вести учёт сотрудников, фиксировать время входа/выхода, формировать табель по форме Т-13 и выгружать отчёты в Excel, CSV для 1С, а также отправлять на печать.

## 🎯 Основные возможности

| Функция | Описание |
|:---|:---|
| **👥 Управление сотрудниками** | Добавление, редактирование, удаление, поиск сотрудников с полной анкетой (ФИО, должность, отдел, дата приёма, статус). |
| **⏱️ Журнал времени** | Фиксация времени входа и выхода каждого сотрудника, возможность добавления примечаний. |
| **📊 Табель (форма Т-13)** | Формирование табеля за выбранный месяц с автоматическим подсчётом отработанных часов. |
| **📤 Экспорт данных** | Выгрузка табеля в форматы: Excel, CSV (для 1С), а также печать. |
| **🔐 Администрирование** | Разграничение прав доступа (администратор/пользователь). |

## 🛠️ Технологический стек

- **Язык программирования:** C#
- **Фреймворк:** .NET Core / .NET Framework (уточни свою версию, например, .NET 6)
- **Интерфейс:** Windows Presentation Foundation (WPF)
- **База данных:** MySQL
- **ORM:** Entity Framework Core (или укажи ADO.NET, если работал через него)
- **Система контроля версий:** Git, GitHub
- **Дополнительно:** Хранимые процедуры, индексы для оптимизации запросов

## 🚀 Установка и запуск

### Требования
- Windows 10/11
- [.NET SDK / Runtime](https://dotnet.microsoft.com/download)
- [MySQL Server](https://dev.mysql.com/downloads/) (версия 5.7 или выше)
- [MySQL Workbench](https://dev.mysql.com/downloads/workbench/) (рекомендуется)

### Шаг 1. Клонирование репозитория
```bash
git clone https://github.com/KOCMOHABT11/WorkingTimeTracking.git
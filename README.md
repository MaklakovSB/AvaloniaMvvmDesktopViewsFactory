# AvaloniaMvvmDesktopViewsFactory (EN)

**A library for building desktop applications on the .NET platform using the AvaloniaUI framework, a centralized view factory, and the MVVM architectural pattern.**

---

## Description

`AvaloniaMvvmDesktopViewsFactory` is an infrastructure library for .NET desktop applications that use AvaloniaUI and follow the MVVM architecture. It provides a centralized mechanism for creating, displaying, and closing *window-views* (`Window`) based on their corresponding *view models* (`ViewModel`), eliminating tight coupling between layers.

The library adheres strictly to the MVVM pattern and is structured as a dependency-injected service (DI). To use it, you must configure your DI container and add the `Microsoft.Extensions.DependencyInjection` package.

View–ViewModel association is performed via the `[ViewFor]` attribute or naming convention (`MainViewModel` ↔ `MainView`).

---

## Features

- Creating the application's main window (`MainWindow`) from a given view model.
- Displaying modal and non-modal windows without explicit type references.
- Dialog support with return values (`ShowDialogViewWithResultAsync`).
- Safe window closing based on the associated view model’s unique identifier (`UID`).
- Automatic disposal of view models that implement `IDisposable` when the window closes.
- Support for multiple windows of the same type displayed simultaneously — each linked to a unique view model via its `UID`.
- Cached `ViewModel → View` resolution for better performance.

---

## Requirements

- .NET 6.0 or .NET 8.0  
- Avalonia UI 11.3.0  
- Microsoft.Extensions.DependencyInjection  
- MVVM architecture with DI container support

---

## License  
MIT License

---

## 📦 Usage

_This section will be expanded later._



# AvaloniaMvvmDesktopViewsFactory (RU)

** Библиотека для построения десктопных приложений на платформе .NET с использованием фреймворка AvaloniaUI, централизованной фабрики представлений и архитектурного паттерна MVVM.**

---

## Описание

`AvaloniaMvvmDesktopViewsFactory` — это инфраструктурная библиотека для .NET-приложений с графическим интерфейсом, использующих AvaloniaUI и архитектуру MVVM. Она предоставляет централизованный механизм создания, отображения и закрытия *окон-представлений* (`Window`), ассоциированных с соответствующими *моделями представления* (`ViewModel`), устраняя жёсткие зависимости между слоями приложения.

Библиотека ориентирована на строгое соблюдение паттерна MVVM и организована в виде сервиса, интегрируемого через механизм внедрения зависимостей (DI). Для использования требуется предварительная настройка контейнера и установка пакета `Microsoft.Extensions.DependencyInjection`.

Сопоставление представлений с моделями осуществляется с помощью атрибута `[ViewFor]` либо на основе соглашения об именовании (`MainViewModel` ↔ `MainView`).

---

## Возможности

- Создание основного окна приложения (`MainWindow`) по заданной модели представления.
- Отображение немодальных и модальных окон без необходимости прямого указания типа представления.
- Поддержка диалоговых окон с возвращаемым результатом (`ShowDialogViewWithResultAsync`).
- Безопасное закрытие окна, связанного с конкретной моделью представления, с учётом уникального идентификатора (`UID`).
- Автоматическое освобождение ресурсов моделей, реализующих `IDisposable`, при закрытии окна.
- Поддержка одновременного отображения нескольких окон одного типа: каждое окно связано со своей моделью и управляется независимо благодаря системе `UID`.
- Кэширование сопоставлений `ViewModel → View` для повышения производительности.

---

## Требования

- .NET 6.0 или .NET 8.0  
- Avalonia UI 11.3.0  
- Microsoft.Extensions.DependencyInjection  
- MVVM-архитектура с поддержкой внедрения зависимостей (DI)

---

## Лицензия  
MIT License

---

## 📦 Использование

_Этот раздел будет дополнен позже._
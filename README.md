# Pacman Mobile — .NET MAUI para Android

Aplicación móvil táctil creada con C# y .NET 8. No utiliza imágenes ni paquetes externos.

## Requisitos

- Windows 10 u 11
- Visual Studio 2022
- Carga de trabajo **Desarrollo de la interfaz de usuario de aplicaciones multiplataforma de .NET**
- Android SDK instalado desde Visual Studio Installer

## Ejecutar en un teléfono Android

1. Activa **Opciones de desarrollador** y **Depuración USB** en el teléfono.
2. Conecta el teléfono por USB y acepta la autorización.
3. Abre `PacmanMobile.csproj` en Visual Studio.
4. Selecciona tu teléfono Android en la barra superior.
5. Presiona `F5`.

## Generar un APK

En una terminal abierta dentro de la carpeta del proyecto:

```powershell
dotnet publish -f net8.0-android -c Release -p:AndroidPackageFormat=apk
```

El APK se crea dentro de:

`bin/Release/net8.0-android/publish/`

## Controles

- Flechas táctiles: mover.
- **Jugar**: comenzar o volver a jugar.
- **Pausa**: detener o continuar.
- **Reiniciar**: comenzar desde cero.

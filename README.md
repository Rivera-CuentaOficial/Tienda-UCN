# Taller de Backend IDWM

El objetivo de este proyecto es levantar una API REST usando ASP.NET 9 y SQLite para create una plataforma de e-comerce llamada Tienda CUN. Incluye autenticacion con JWT, manejo de perfiles, productos y carrito de compra.

Se utiliza una patron Repository para mantener una arquitectura limpia, clara separacion de responsabilidades, y facilidad de mantencion.

Para el hosting de imagenes se utiliza Cloudinary y Resend para el envio de correos.

# Instalacion

Para la ejecucion del proyecto se tienen los siguientes requerimientos:
-   [Visual Studio Code 1.89.1+](https://code.visualstudio.com/)
-   [ASP .Net Core 9](https://dotnet.microsoft.com/en-us/download)
-   [git 2.45.1+](https://git-scm.com/downloads)
-   [Postman](https://www.postman.com/downloads/)

Una vez que los requerimientos esten instalados, se puede proceder a clonar el repositorio.


# Quick Start
1. Clona el repositorio a tu maquina local usando la terminal:
```bash
    git clone https://github.com/Rivera-CuentaOficial/Tienda-UCN.git

```
2. Navega a la carpeta donde clonaste el proyecto:
```bash
    cd Tienda-UCN-API
```
3. Abre el proyecto con Visual Studio Code:
```bash
    code .
```

4. Para correr el proyecto vas a necesitar un archivo `appsettings.json`:
```bash
    cp appsettings.Example.json appsettings.json
```
  Y luego reemplaza tus credenciales en los espacios correspondientes:

**Required configuration:**
```json
{
  "Cloudinary": {
    "CloudName": "Cloudinary-Name",
    "ApiKey": "API-KEY",
    "ApiSecret": "API-SECRET"
  },
  "JWTSecret": "JWT-SECRET",
  "ResendAPIKey": "RESEND-API-KEY",
}
```
- Tu `Cloudinary-Name` esta en el dashboard de Cloudinary.
- Lo mismo con tu `API-KEY` y `API-SECRET`
- Puedes crear tu propia `JWTSecret` mientras sea una cadena de a lo menos 32 caracteres de largo. Puedes usar un generador online como [este](https://jwtsecrets.com)
- Reemplaza `RESEND-API-KEY` con tu propia llave de Resend. La puedes encontrar en el siguiente link: [Resend - Getting Started](https://resend.com/docs/send-with-dotnet)

5. Restaura las dependencias del proyecto con el siguiente comando:
```bash
    dotnet restore
```
6. Para ejecutar el proyecto, utiliza el siguiente comando en la terminal de VSC:
```bash
    dotnet run
```
# Postman
Se incluye un archivo de coleccion de Postman para realizar pruebas.

# Integrantes
Sebastian Rivera - srg004@alumnos.ucn.cl
Bayron Cruz - bayron.cruz@alumnos.ucn.cl











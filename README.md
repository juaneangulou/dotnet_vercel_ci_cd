# API REST con .NET 10, Swagger y Vercel

Solucion ASP.NET Core con Clean Architecture, cuatro proyectos de produccion,
pruebas unitarias/de integracion y GitHub Actions para CI/CD.
La API de ejemplo consulta un catalogo y calcula cotizaciones **sin estado**.
No pretende ser un CRUD con base de datos ni implementa autenticacion.

## 1. Requisitos

- SDK .NET 10 estable. `global.json` acepta el ultimo feature band de .NET 10.
- Git; Node.js 22+ y npm solamente para el CLI de despliegue.
- Cuenta de GitHub y cuenta/proyecto de Vercel.
- Docker Desktop con contenedores Linux, solo para verificar la imagen localmente.

### Compatibilidad real con Vercel

Se utiliza el **Container Runtime oficial de Vercel, actualmente en beta**,
disponible segun su documentacion en todos los planes. Vercel detecta
`Dockerfile.vercel` en la raiz, construye la imagen OCI y enruta las peticiones
al servidor ASP.NET Core. No se utiliza un runtime .NET comunitario ni un
adaptador Node.js. Confirma la disponibilidad en tu cuenta antes de desplegar.

Referencias:
- [Container Runtime: deteccion, puerto, limites y beta](https://vercel.com/docs/functions/runtimes/container)
- [Vercel CLI deploy](https://vercel.com/docs/cli/deploy)
- [Variables de entorno de Vercel](https://vercel.com/docs/environment-variables)

El contenedor escucha en `PORT` (8080 por defecto en la imagen). Vercel termina
TLS en el borde; por eso la API no redirige HTTP a HTTPS dentro del contenedor.
La imagen final usa el usuario no-root de las imagenes oficiales de .NET.
No se requiere `vercel.json`: se usa la autodeteccion oficial del Dockerfile.

## 2. Estructura y dependencias

```text
CleanApi.sln
src/
  CleanApi.Domain/          Entidades y reglas de negocio
  CleanApi.Application/     Casos de uso, contratos y DTO de salida
  CleanApi.Infrastructure/  Implementaciones de acceso a datos
  CleanApi.Api/             HTTP, Swagger, configuracion y composicion
tests/
  CleanApi.Tests/           Pruebas unitarias y de integracion
.github/workflows/ci-cd.yml
Dockerfile.vercel
```

```text
Application ----> Domain
Infrastructure -> Application -> Domain
Api ------------> Application
Api ------------> Infrastructure (solo composicion de dependencias)
Tests ----------> Api (y sus dependencias)
```

El dominio no depende de ASP.NET Core, bases de datos ni paquetes externos.
Consulta el README de cada proyecto para saber que agregar en cada capa:
[Domain](src/CleanApi.Domain/README.md),
[Application](src/CleanApi.Application/README.md),
[Infrastructure](src/CleanApi.Infrastructure/README.md),
[Api](src/CleanApi.Api/README.md) y [Tests](tests/CleanApi.Tests/README.md).

## 3. Ejecutar localmente, paso a paso

Desde PowerShell, en la raiz de la solucion:

```powershell
dotnet --version
dotnet restore CleanApi.sln
$env:App__Name = "Mi API REST"
$env:App__SupportEmail = "soporte@example.com"
dotnet run --project .\src\CleanApi.Api --launch-profile http
```

- Swagger UI: http://localhost:5091/swagger
- OpenAPI JSON: http://localhost:5091/openapi/v1.json
- Salud: http://localhost:5091/health

El perfil local establece `ASPNETCORE_ENVIRONMENT=Development`. En ese ambiente
Swagger siempre esta activo. Para HTTPS local puedes usar el perfil `https`
despues de ejecutar `dotnet dev-certs https --trust`.

### Variables de entorno

| Variable | Obligatoria | Uso |
| --- | --- | --- |
| `App__Name` | Si | Nombre publico de la API; 1 a 100 caracteres |
| `App__SupportEmail` | Si | Email publico de soporte valido |
| `Swagger__Enabled` | No | `true` publica Swagger/OpenAPI fuera de Development; por defecto desactivado |
| `PORT` | No | Puerto HTTP de 1 a 65535; la imagen establece 8080 |
| `ASPNETCORE_ENVIRONMENT` | No | La imagen usa `Production` |

`__` representa el separador de configuracion `:` de .NET.
Las variables de entorno sobrescriben `appsettings.json`.
Las dos variables obligatorias se validan al arrancar: si faltan o son
invalidas, el proceso falla explicitamente. No hay valores de produccion
inventados ni secretos en el repositorio.

`.env.example` es una referencia, **no se carga automaticamente**.
Establece las variables en PowerShell, user-secrets o Vercel.
`App__Name` y `App__SupportEmail` son informacion publica devuelta por
`GET /api/info`; nunca pongas credenciales en esos campos.

## 4. Endpoints y ejemplos

| Metodo | Ruta | Resultado |
| --- | --- | --- |
| GET | `/api/info` | Nombre y contacto configurados |
| GET | `/api/products` | Catalogo de ejemplo |
| GET | `/api/products/{id}` | Producto o 404 |
| POST | `/api/products/{id}/quotes` | Cotizacion, sin persistencia |
| GET | `/health` | Liveness del proceso |
| GET | `/openapi/v1.json` | Contrato, cuando Swagger esta activo |
| GET | `/swagger` | Interfaz Swagger, cuando esta activa |

```powershell
Invoke-RestMethod http://localhost:5091/api/products
Invoke-RestMethod `
  -Method Post `
  -Uri http://localhost:5091/api/products/11111111-1111-1111-1111-111111111111/quotes `
  -ContentType "application/json" `
  -Body '{"quantity":3}'
```

Respuesta de cotizacion:

```json
{
  "productId": "11111111-1111-1111-1111-111111111111",
  "quantity": 3,
  "unitPrice": 49.90,
  "total": 149.70
}
```

La cantidad debe estar entre 1 y 1000. La validacion HTTP devuelve 400 con
`ValidationProblemDetails`; recursos inexistentes devuelven 404 con
`ProblemDetails`. Los errores inesperados usan el manejador centralizado de
ASP.NET Core, con registro de errores y sin publicar stack traces en Production.
Se usa `decimal` para importes; el ejemplo no define moneda, impuestos ni redondeo.

## 5. Compilar y probar

```powershell
dotnet build CleanApi.sln --configuration Release
dotnet test CleanApi.sln --configuration Release --no-build
```

Las advertencias se tratan como errores. Las pruebas cubren reglas del dominio,
cancelacion, cotizaciones, rutas HTTP, validacion, configuracion obligatoria,
ProblemDetails, contrato OpenAPI y habilitacion de Swagger en Production.

### Probar la misma imagen de despliegue

Inicia Docker Desktop (motor Linux) y ejecuta:

```powershell
docker build --file Dockerfile.vercel --tag clean-api:local .
docker run --rm --name clean-api-local -p 8080:8080 `
  -e App__Name="Mi API REST" `
  -e App__SupportEmail="soporte@example.com" `
  -e Swagger__Enabled=true `
  clean-api:local
```

Abre http://localhost:8080/swagger y http://localhost:8080/health.
Deten el contenedor con Ctrl+C. Dockerfile usa rutas POSIX porque se ejecuta
en un contenedor Linux, aunque desarrolles desde Windows.

## 6. Crear y configurar el proyecto Vercel

1. Instala el CLI fijado por el repositorio: `npm ci`.
2. Ejecuta `npx vercel login` y autentica tu cuenta.
3. En la raiz ejecuta `npx vercel link`; crea o selecciona un proyecto.
4. En Vercel > Project Settings selecciona **Framework Preset: Other** y
   **Root Directory: raiz del repositorio**. No agregues un comando `npm run build`,
   un output estatico ni un runtime comunitario: el entrypoint es
   `Dockerfile.vercel`. Elimina overrides incompatibles de un proyecto existente.
5. En Project Settings > Environment Variables agrega, para **Production**:
   - `App__Name`: el nombre publico deseado.
   - `App__SupportEmail`: el correo publico deseado.
   - `Swagger__Enabled`: `true` si deseas publicar la documentacion.
   - `PORT`: `8080` (explicito para mantener alineados Vercel y la imagen).
6. Si deseas previews manuales, agrega tambien las variables en **Preview**.
7. Ejecuta `npx vercel deploy --prod` para un primer despliegue manual opcional.
   Vercel debe construir el contenedor y mostrar una URL.
8. Comprueba `https://TU-DOMINIO/health`, `/api/info`, `/api/products` y
   `/swagger` si lo habilitaste. Para una cotizacion usa el POST del ejemplo.

Los cambios de variables en Vercel requieren un nuevo despliegue. No uses
`--build-env` para secretos de ejecucion ni los incrustes en la imagen.
Si Deployment Protection esta activo, autentica las comprobaciones con Vercel;
una respuesta de su pagina de acceso no es un error del endpoint ASP.NET Core.

## 7. Configurar GitHub Actions CI/CD

1. Sube esta solucion a GitHub y usa `main` como rama de produccion.
2. En GitHub > Settings > Environments crea el ambiente **production**.
   Puedes exigir aprobacion y limitar el despliegue a `main`.
3. Crea un token desde [Vercel > Account Settings > Tokens](https://vercel.com/account/settings/tokens):
   pulsa **Create Token**, asigna un nombre como `github-actions-clean-api`,
   selecciona el scope de la cuenta/equipo propietario del proyecto y define
   una caducidad. Copia el token al crearlo y guardalo en GitHub como
   `VERCEL_TOKEN`, siguiendo el paso 5. No lo compartas ni lo guardes en archivos
   del repositorio. Renuevalo antes de que expire; si se expone, revocalo.
4. Despues de `npx vercel link`, consulta localmente `.vercel/project.json`:
   `orgId` corresponde a `VERCEL_ORG_ID` y `projectId` a `VERCEL_PROJECT_ID`.
   No subas esa carpeta ni el token al repositorio.
5. En los **Environment secrets** de `production` agrega:

   | Secret | Contenido |
   | --- | --- |
   | `VERCEL_TOKEN` | Token de despliegue |
   | `VERCEL_ORG_ID` | `orgId` del equipo/cuenta |
   | `VERCEL_PROJECT_ID` | `projectId` del proyecto |

6. Conserva las variables de la API en **Vercel**, no en estos secrets.
7. Si conectaste la integracion Git de Vercel, desactiva sus despliegues
   automaticos en la configuracion Git del proyecto o desconecta el repositorio.
   Este ejemplo usa Actions como unica autoridad para desplegar despues de CI;
   no dejes un segundo flujo que despliegue antes de terminar las pruebas.
8. Abre un pull request hacia `main`: se ejecutan restore, build Release,
   pruebas, construccion Docker y smoke test HTTP de la imagen no-root.
   Los PR no reciben credenciales ni despliegan.
9. Al fusionar en `main`, CI vuelve a validar y, solo si termina bien, CD ejecuta
   `vercel deploy --prod --yes`. Vercel construye la imagen desde ese checkout;
   no se usa `--prebuilt`, que omite la construccion remota necesaria.
   Despues se verifican `/health` y el JSON de `/api/products` en el despliegue
   real mediante `vercel curl` (compatible con Deployment Protection). Si la
   API falla al iniciar o devuelve una respuesta invalida, CD falla aunque
   Vercel haya aceptado la construccion.
10. Consulta la URL en el resumen de Actions y el ambiente `production`.
    Tambien puedes iniciar el workflow manualmente desde `main`.

El pipeline no descarga variables de produccion durante CI: usa datos ficticios
para las pruebas. `package-lock.json` fija el arbol del CLI y `npm ci` lo respeta.
Los despliegues de una misma rama se serializan para evitar cancelaciones
a mitad de publicacion.

### Dependencias del CLI de despliegue

El CLI `vercel@64.3.1` es una herramienta de desarrollo, no forma parte de la
imagen ni del proceso .NET. Su arbol de dependencias presenta avisos conocidos
en `npm audit`, incluyendo severidades altas/criticas. Fijar versiones no
elimina esas vulnerabilidades. Revisa `npm audit` antes de autorizar CD, actualiza
el CLI/lockfile cuando el proveedor publique correcciones y limita el token.
No se aplican overrides de versiones mayores ni `npm audit fix --force` porque
pueden romper el CLI o quitar el soporte del runtime de contenedores.

Recomendado: protege `main`, exige el check **Build, test and container smoke test**
y revisiones. Usa las herramientas de Vercel para promover un despliegue previo
si necesitas rollback; no intentes recuperar datos desde el filesystem del contenedor.

## 8. Como extender la solucion

Ejemplo para agregar pedidos:

1. Define `Order` y sus invariantes en Domain.
2. Define contratos y casos de uso de creacion/consulta en Application.
3. Implementa repositorios y transacciones con una base de datos externa en
   Infrastructure. Lee la conexion desde configuracion, nunca desde el dominio.
4. Registra implementaciones en el composition root de Api y agrega controladores,
   contratos de entrada, validaciones y respuestas HTTP documentadas.
5. Agrega pruebas de reglas/casos de uso y pruebas HTTP de exito y error.

El catalogo demo es fijo y de solo lectura. Las cotizaciones no se guardan.
**No agregues almacenamiento mutable en memoria, archivos locales ni SQLite
como persistencia de produccion en Vercel**: las instancias escalan y se descartan.
Usa una base externa, migraciones controladas en un job separado e idempotencia
para escrituras reintentables. `/health` solo prueba liveness; al agregar una base,
crea readiness con verificaciones reales y sin exponer detalles sensibles.

Antes de manejar datos privados agrega autenticacion/autorizacion y limites de
peticiones apropiados. Configura CORS solo para origenes concretos si tienes
un frontend en otro dominio. No se habilita CORS abierto por defecto.
No confies indiscriminadamente en cabeceras forwarded; configura proxies
conocidos si mas adelante necesitas reconstruir esquema/IP/origen.

## 9. Solucion de problemas

- **Falla al arrancar**: revisa `App__Name`, `App__SupportEmail`, `PORT` y los logs.
- **Swagger devuelve 404 en Production**: establece `Swagger__Enabled=true`
  y redespliega. Desactivalo o protegelo en APIs privadas.
- **Vercel intenta un build Node o pide output estatico**: revisa preset, raiz y
  overrides; debe detectar `Dockerfile.vercel`.
- **El runtime de contenedores no esta disponible en tu cuenta**: confirma la
  beta con Vercel. No reemplaces esto silenciosamente por un runtime antiguo;
  la misma imagen puede alojarse en otra plataforma que soporte contenedores.
- **Docker local no conecta**: inicia Docker Desktop y verifica el motor Linux.
- **No despliega Actions**: verifica rama `main`, aprobacion de `production`,
  los tres secrets, permisos del token y resultado del job CI.
- **401/403 externo**: verifica primero Deployment Protection y permisos de Vercel.

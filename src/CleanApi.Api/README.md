# Api

Es el adaptador HTTP y composition root. Aqui estan `Program`, los controladores,
contratos HTTP de entrada, configuracion tipada, validacion, ProblemDetails,
OpenAPI/Swagger y health checks.

Agregar: endpoints delgados, autenticacion/autorizacion, politicas CORS concretas,
middleware y registros de DI. Mapear los resultados del caso de uso a HTTP.
No agregar: consultas SQL, acceso directo a bases de datos, calculos de negocio
ni secretos en appsettings o en controladores.

Referencia Application para ejecutar casos de uso e Infrastructure solamente para
componer implementaciones. `AppOptions` valida nombre/email al arrancar.
`/api/info` devuelve esos valores publicos; no debe exponer secretos.

Swagger esta activo en Development y requiere `Swagger__Enabled=true` en
Production. TLS termina en Vercel; `PORT` permite usar el puerto del runtime.
No se registra informacion sensible ni se devuelven excepciones internas al cliente.

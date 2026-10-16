# Tests

`ProductTests` cubre reglas del dominio, limites, importes, un caso de uso y
cancelacion del repositorio. `ApiTests` usa `WebApplicationFactory<Program>` con
configuracion de prueba, sin red externa ni secrets de produccion.

Agregar pruebas de cada regla/caso de uso, respuestas HTTP, errores, validacion y
contratos OpenAPI. Si crece el sistema, separa pruebas unitarias, integracion de
infraestructura y arquitectura en proyectos independientes.

Desde la raiz:

```powershell
dotnet test CleanApi.sln --configuration Release
```

CI tambien construye y ejecuta la imagen Linux para verificar el arranque real
como usuario no-root y los endpoints de salud/documentacion.

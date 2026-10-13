# Agenda Profesional

Aplicación de consola en C# (.NET 10) para que una consultora gestione las personas de contacto y las empresas con
las que colabora: alta, listado, búsqueda, modificación y baja de ambas, y la relación entre ellas (una persona
pertenece a cero o una empresa; una empresa tiene cero, una o muchas personas). Los datos se guardan en un archivo JSON.

## Cómo ejecutarla

Desde la carpeta raíz del repositorio:

```bash
dotnet build
dotnet run --project src/AgendaProfesional.Console
```

- Los datos se guardan en `datos/agenda.json` (dentro de la carpeta desde la que se ejecuta). La primera vez, si no
  existe, se crea con datos de ejemplo. **Para volver a los datos de ejemplo, borra ese archivo.**
- `dotnet run --project src/AgendaProfesional.Console -- --sin-archivo` trabaja solo en memoria, sin leer ni guardar nada.
- En VS Code abre la carpeta raíz: la extensión C# Dev Kit detecta `AgendaProfesional.sln`.

Requisitos: .NET SDK 10.

## Estructura (Fase 5)

```
AgendaProfesional/
├── README.md
├── AgendaProfesional.sln
└── src/
    └── AgendaProfesional.Console/
        ├── AgendaProfesional.Console.csproj
        ├── Program.cs                  Punto de entrada: crea las piezas y arranca la consola
        ├── Models/
        │   ├── Persona.cs              Datos de una persona y su IdEmpresa opcional
        │   ├── Empresa.cs              Datos de una empresa y su Estado (0 baja / 1 activa)
        │   └── IEntidad.cs             Lo común a ambas (Id, Codigo, Clonar)
        ├── Services/
        │   ├── AgendaService.cs        Operaciones de negocio
        │   ├── Resultado.cs            Éxito o lista de errores de una operación
        │   └── DatosDeEjemplo.cs       Datos iniciales para la demo
        ├── Infrastructure/
        │   ├── IRepositorios.cs        Contratos de acceso a datos
        │   ├── RepositorioBase.cs      Alta, modificación, baja y consultas comunes
        │   ├── RepositorioPersonas.cs  Consultas propias de personas
        │   ├── RepositorioEmpresas.cs  Consultas propias de empresas
        │   ├── ContextoAgenda.cs       Carga, comprueba la consistencia y guarda tras cada cambio
        │   ├── DatosAgenda.cs          Lo que se guarda en el archivo
        │   ├── IAlmacenAgenda.cs       Dónde se guarda
        │   ├── AlmacenJson.cs          En un archivo JSON (escritura segura)
        │   └── AlmacenMemoria.cs       Solo en memoria (pruebas)
        ├── UI/
        │   ├── ConsoleUI.cs            Menú principal y avisos de inicio
        │   ├── PantallaEntidad.cs      Alta, listado, búsqueda y modificación comunes
        │   ├── PantallaPersonas.cs     Campos y baja de personas
        │   ├── PantallaEmpresas.cs     Campos, baja lógica y recuperación de empresas
        │   ├── PantallaRelaciones.cs   Asignar, desvincular y consultas de la relación
        │   └── Componentes/            Menu, Tabla, Campo y Consola (lectura y mensajes)
        └── Utils/
            ├── InputValidator.cs       Reglas de formato y normalización de los datos
            ├── CodigoId.cs             Id numérico ⇄ código P001 / E001
            ├── Busqueda.cs             Búsqueda por Id o por texto
            └── Texto.cs                Comparación sin mayúsculas ni tildes
```

## Fase 5: modelo orientado a objetos y responsabilidades

```
Program ──crea──> ConsoleUI ──usa──> AgendaService ──usa──> IRepositorios ──implementa──> Infrastructure (JSON)
                      │                    │                                                  │
                      └────────────────────┴──────────── Models (Persona, Empresa) ───────────┘
                                 Utils (InputValidator, CodigoId...) lo usan todas las capas
```

| Componente | Responsabilidad |
|---|---|
| **Persona** | Datos de una persona y su empresa asignada mediante `IdEmpresa` opcional (`int?`). |
| **Empresa** | Datos de una empresa y su `Estado` (baja lógica). |
| **AgendaService** | Operaciones de negocio: altas, búsquedas, modificaciones, asignaciones, bajas y consultas. Aplica las reglas (validación, correo y CIF únicos, empresa activa, no borrar empresas con personas). No escribe en pantalla ni sabe cómo se guardan los datos. |
| **ConsoleUI** | Muestra los menús, pide los datos y presenta los resultados. **No toca las colecciones**: todo lo pide a `AgendaService`. Delega cada bloque en `PantallaPersonas`, `PantallaEmpresas` y `PantallaRelaciones`. |
| **Program** | Punto de entrada: crea el almacén, los repositorios, el servicio y la consola, y arranca. Unas 20 líneas, sin lógica. |
| **InputValidator** | Reglas de formato de cada dato. Las usan la consola (al teclear) y el servicio (antes de guardar): las reglas están en un solo sitio. |
| **Infrastructure** | Persistencia aislada: repositorios, contexto y almacén JSON. Ni los menús ni el servicio saben que hay un archivo. |

### Persistencia en JSON (capa Infrastructure)

Toda la agenda se guarda en **un único archivo** `datos/agenda.json`:

```json
{
  "UltimoIdPersona": 4,
  "UltimoIdEmpresa": 3,
  "Personas": [
    { "Id": 1, "Nombre": "Laura", "Apellidos": "Martínez Ruiz", "Telefono": "612345678",
      "Correo": "laura.martinez@techsolutions.es", "IdEmpresa": 1, "Cargo": "Jefa de proyecto" }
  ],
  "Empresas": [
    { "Id": 1, "NombreComercial": "TechSolutions S.L.", "Cif": "B12345678", "Telefono": "912345678",
      "Correo": "info@techsolutions.es", "Direccion": "Calle Mayor 10, Madrid", "Estado": 1 }
  ]
}
```

| Decisión | Justificación |
|---|---|
| **Un solo archivo** para personas y empresas | Se guardan juntas, así nunca queda una persona apuntando a una empresa de otro archivo que no llegó a guardarse. |
| **Contadores de Id en el archivo** | Un Id borrado no se reutiliza aunque se cierre el programa, igual que un autoincremental de base de datos. |
| **`ContextoAgenda` (unidad de trabajo)** | Carga los datos una vez, los repositorios trabajan sobre él y guarda después de cada cambio. Es el mismo papel que tendría un `DbContext` con base de datos. |
| **Escritura segura** | Se escribe en `agenda.json.tmp` y después sustituye al original: si el programa se corta a mitad, el archivo bueno sigue intacto. |
| **Si no se puede guardar** (archivo bloqueado, sin permisos) | Se muestra `[ERROR]` y se vuelve al menú; el programa no termina. |
| **`AlmacenMemoria`** | Misma interfaz sin archivo, para pruebas (`--sin-archivo`). |

### Comprobaciones de consistencia al cargar

`ContextoAgenda` revisa el archivo al arrancar (por si se editó a mano) y muestra un `[AVISO]` por cada problema:

| Problema | Qué se hace |
|---|---|
| JSON dañado o vacío | Se aparta como `agenda.json.danado-FECHA` (no se borra) y se empieza con datos de ejemplo. |
| Contador de Id por debajo del Id más alto | Se ajusta, para que no se repitan Ids. |
| Id repetido o no válido | Se le asigna un Id nuevo. |
| Persona apuntando a una empresa inexistente o dada de baja | Se queda sin empresa. |
| Datos con formato incorrecto (correo, CIF…) | No se tocan; solo se avisa. |

Si hubo reparaciones, el archivo se vuelve a guardar ya corregido.

**Pruebas sugeridas:** con el programa cerrado, edita `datos/agenda.json` y cambia un `IdEmpresa` a `99`, repite un `Id`,
pon un correo sin `@` o borra una llave para romper el JSON. Al arrancar verás el aviso correspondiente.

### Equivalencia de nombres (Fases 1-4 → Fase 5)

Las secciones de las fases anteriores describen el código de su momento. En la Fase 5 se reorganizó así
(con `git mv`, de modo que el historial de cada archivo se conserva):

| Antes | Ahora |
|---|---|
| `Proyecto1.csproj` | `src/AgendaProfesional.Console/AgendaProfesional.Console.csproj` + `AgendaProfesional.sln` |
| `Modelos/` | `Models/` |
| `Datos/IRepositorio*.cs` | `Infrastructure/IRepositorios.cs` |
| `Datos/RepositorioMemoria.cs`, `Repositorio*Memoria.cs` | `Infrastructure/RepositorioBase.cs`, `RepositorioPersonas.cs`, `RepositorioEmpresas.cs` (ahora guardan en JSON a través de `ContextoAgenda`) |
| `Servicios/ServicioPersonas.cs` + `ServicioEmpresas.cs` | `Services/AgendaService.cs` (un único servicio) |
| `Servicios/Validador.cs` + `ValidadorPersona.cs` + `ValidadorEmpresa.cs` | `Utils/InputValidator.cs` |
| `Interfaz/Pantalla.cs` | `UI/Componentes/Consola.cs` |
| `Interfaz/MenuEntidad.cs`, `MenuPersonas.cs`, `MenuEmpresas.cs`, `MenuRelaciones.cs` | `UI/PantallaEntidad.cs`, `PantallaPersonas.cs`, `PantallaEmpresas.cs`, `PantallaRelaciones.cs` |
| (menú principal en `Program.cs`) | `UI/ConsoleUI.cs` |
| `Utilidades/` | `Utils/` |

## Decisiones de diseño generales

| Decisión | Justificación |
|---|---|
| **Id `int` interno, código `P001` / `E001` visible** | Como un autoincremental de BD. El código es solo cómo se muestra y se busca; no depende del nombre. |
| **Los repositorios trabajan con copias** | Modificar un objeto no guarda nada hasta llamar a `Actualizar`, igual que con una base de datos. |
| **Validación en el servicio** | La consola valida cada dato al escribirlo, pero el servicio vuelve a validar antes de guardar: ninguna interfaz futura puede guardar datos incorrectos. |
| **`Resultado`** en vez de mensajes por pantalla | El servicio devuelve los errores y la interfaz decide cómo mostrarlos. |
| **`Campo<T>`: cada dato definido una sola vez** | Alta, ficha, tabla y menú de modificar se generan a partir de la misma lista. |
| **`PantallaEntidad<T>`, `RepositorioBase<T>`, `Menu`, `Tabla`** | Código común escrito una vez para personas y empresas. |
| Campo añadido: **Cargo** | En una agenda de consultora interesa el rol de cada contacto. Es opcional. |
| Menú sin limpiar pantalla | Toda la ejecución queda en la terminal, útil para las capturas de evidencia. |

## Validaciones

| Dato | Regla |
|---|---|
| Opción de menú | Si no es un número o no existe, muestra `[ERROR]` y repite el menú. El programa nunca se cierra por una entrada incorrecta. |
| Nombre y apellidos | Obligatorios y sin dígitos. |
| Teléfono | Obligatorio. Solo dígitos (opcionalmente con `+`), entre 9 y 15. Se admiten espacios y guiones al escribir; se guarda solo el número. |
| Correo | Obligatorio. Formato `usuario@dominio.ext`, sin espacios, no repetido. Se guarda en minúsculas. |
| Confirmaciones | Solo acepta S/SI/SÍ o N/NO. |

Si un dato es incorrecto se repite **solo ese campo**.

## Guion para la demostración de personas (evidencia)

Desde la Fase 2 estas opciones están en `1. Gestión de personas` del menú principal.

El programa arranca con 4 personas de ejemplo (P001–P004).

1. **Menú:** escribe `hola` y después `9` → error y el menú se repite.
2. **Listar (2):** tabla ordenada por apellidos.
3. **Alta (1):** prueba teléfono `abc` y correo `sin-arroba` para ver los errores; completa y confirma con `S` → `P005`.
4. **Buscar (3):** `garcia` (sin tilde) y después `p5`.
5. **Modificar (4):** primero `P099` → no existe. Vuelve a entrar y busca `ez` → varias coincidencias; elige `P002`, opción 3 (teléfono), confirma y termina con `0`.
6. **Baja (5):** elimina `P004` confirmando con `S`.
7. **Listar (2)** y **Salir (0)**.

> Las secciones siguientes documentan cada fase tal como se entregó. Los nombres de archivo de las Fases 1 a 4
> corresponden a la estructura anterior; consulta la tabla «Equivalencia de nombres» de la Fase 5.

## Fase 2: gestión de empresas

Menú principal → `1. Gestión de personas` / `2. Gestión de empresas`. Cada submenú vuelve al principal con `0`
sin cerrar la aplicación.

### Archivos nuevos o cambiados

| Archivo | Qué hace |
|---|---|
| `Modelos/Empresa.cs` | Datos de la empresa y su `Estado` (enum `Baja = 0`, `Activa = 1`). |
| `Modelos/IEntidad.cs` | Lo común a Persona y Empresa (`Id`, `Codigo`, `Clonar`). |
| `Modelos/Persona.cs` | `Empresa` (texto) pasa a `IdEmpresa` (`int?`): relación 1:N. |
| `Datos/IRepositorio.cs`, `RepositorioMemoria.cs` | Almacenamiento genérico: se escribe una vez y sirve para ambas. |
| `Datos/IRepositorioEmpresas.cs`, `RepositorioEmpresasMemoria.cs` | Consultas propias de empresas (por CIF, por texto). |
| `Servicios/ServicioEmpresas.cs` | Alta, modificación, baja lógica, recuperación y CIF único. |
| `Servicios/Validador.cs`, `ValidadorEmpresa.cs` | Reglas compartidas (teléfono, correo) y propias (CIF). |
| `Interfaz/MenuEntidad.cs` | Alta, listado, búsqueda y modificación comunes a los dos menús. |
| `Interfaz/MenuEmpresas.cs` | Campos de empresa, baja y recuperación. |
| `Interfaz/Campo.cs` | Antes `CampoPersona`; ahora genérico. |

### Decisiones de diseño

| Decisión | Justificación |
|---|---|
| **Baja lógica con `Estado`** | Dar de baja pone `Estado = 0`: la empresa deja de aparecer en listados y búsquedas, pero el registro se conserva. "Recuperar" la devuelve a `1`. |
| **Solo se da de baja si no tiene personas** | Si hay personas vinculadas, se muestran y se bloquea la baja: nunca quedan personas apuntando a una empresa oculta. |
| **Persona vinculada por `IdEmpresa`** | Al dar de alta o modificar una persona se elige una empresa activa por su Id (`E001`) o ninguna. Se enseña el nombre, se guarda el Id. |
| **CIF con formato y único** | Letra + 7 dígitos + dígito o letra de control. No se repite, tampoco con empresas dadas de baja (el mensaje indica que se puede recuperar). |
| **`MenuEntidad<T>` y `RepositorioMemoria<T>`** | Personas y empresas comparten el mismo código de pantallas y de almacenamiento; cada una solo define sus campos y sus textos. |
| **Sigue en memoria** | El enunciado de esta fase no pide base de datos. `Estado` ya queda como la columna que tendrá la tabla. |

### Validaciones de empresa

| Dato | Regla |
|---|---|
| Nombre comercial | Obligatorio. |
| CIF | Obligatorio, formato `B12345678`, no repetido. Se guarda en mayúsculas. |
| Teléfono y correo | Las mismas reglas que en personas. |
| Dirección | Obligatoria. |

### Guion para la demostración

Arranca con 3 empresas (E001–E003) y 4 personas. E003 no tiene personas vinculadas.

1. **Menú principal:** entra en `2` (empresas), vuelve con `0`, entra en `1` (personas), vuelve con `0`.
2. **Empresas → Listar (2).**
3. **Alta (1):** prueba CIF vacío, CIF `123` y CIF repetido `B12345678`; nombre comercial vacío. Completa y confirma → `E004`.
4. **Buscar (3):** `tech` y `e2`.
5. **Modificar (4):** `E002`, cambia el teléfono.
6. **Baja (5) bloqueada:** `E001` → muestra las personas vinculadas y no deja.
7. **Baja (5) correcta:** `E003` → confirmar. **Listar (2):** ya no aparece.
8. **Recuperar (6):** `E003` → confirmar. **Listar (2):** vuelve a aparecer.
9. **Personas → Alta (1):** al llegar a Empresa se listan las disponibles; escribe `E004`.

## Fase 3: relación persona - empresa

Una persona tiene **cero o una** empresa; una empresa tiene **cero, una o muchas** personas (relación 1:N).
Se guarda en `Persona.IdEmpresa` (`null` = sin empresa).

Menú principal → `3. Relaciones persona - empresa` (`Interfaz/MenuRelaciones.cs`):

| Operación | Comportamiento | Dónde |
|---|---|---|
| Asignar o cambiar empresa | Se elige una persona y una empresa activa por su Id (o texto). Pide confirmación. | Opción 1 · `ServicioPersonas.AsignarEmpresa` |
| Desvincular persona | La persona conserva sus datos y queda sin empresa. Pide confirmación. | Opción 2 · `ServicioPersonas.Desvincular` |
| Ver empresa de una persona | Muestra la ficha de su empresa, o avisa si no tiene. También sale en la columna Empresa del listado de personas. | Opción 3 |
| Ver personas de una empresa | Lista la plantilla de la empresa elegida. | Opción 4 · `ServicioPersonas.ObtenerPorEmpresa` |
| Resumen | Cada empresa activa con sus personas, y al final las personas sin empresa. | Opción 5 |
| Borrar empresa relacionada | **No se permite**: muestra `[ERROR]` con las personas vinculadas y la política. | Empresas → 5 · `ServicioEmpresas.DarDeBaja` |

### Política de borrado elegida

**Una empresa con personas no se puede borrar** (equivale a `ON DELETE RESTRICT`). Hay que desvincular o
cambiar de empresa a sus personas antes. Motivo: borrarla en cascada eliminaría contactos válidos, y dejarlos
sin empresa en silencio perdería información sin que el usuario lo decida. Así cada cambio es explícito
y confirmado. La comprobación está en el servicio, de modo que ninguna interfaz futura puede saltársela.

Mensaje que muestra la consola:

```
[ERROR] No se puede dar de baja E001 - TechSolutions S.L.: tiene 2 persona(s) vinculada(s).
    P001 - Laura Martínez Ruiz
    P003 - Ana Fernández Gil
[AVISO] Política: una empresa con personas no se puede borrar. Desvincúlalas o cámbialas de empresa antes (menú principal -> 3. Relaciones persona - empresa).
```

### Guion para la demostración

1. **Relaciones → 5:** resumen inicial (E001 con 2 personas, E002 con 1, E003 con 0, Javier sin empresa).
2. **Empresas → 5:** intenta borrar `E001` → `[ERROR]` con sus personas y la política.
3. **Relaciones → 1:** asigna `P004` (Javier) a `E003`.
4. **Relaciones → 1:** cambia `P001` (Laura) de `E001` a `E002`.
5. **Relaciones → 2:** desvincula `P003` (Ana) → `E001` queda sin personas.
6. **Relaciones → 3:** empresa de `P001` → ficha de E002. **Relaciones → 4:** personas de `E002`.
7. **Empresas → 5:** ahora `E001` sí se puede dar de baja.
8. **Relaciones → 5:** resumen final.

## Fase 4: organización mediante funciones

El programa se refactoriza sin cambiar lo que hace. Cada responsabilidad está en una función con un nombre
que explica qué hace, separando **presentación de menús**, **lectura y validación de datos** y **operaciones de gestión**.

### Reparto de responsabilidades de las funciones principales

| Bloque | Función | Responsabilidad | Recibe → Devuelve |
|---|---|---|---|
| **Coordinación** | `Program.cs` (main) | Crea repositorios, servicios y menús, los conecta y arranca el menú principal. No tiene lógica. | — |
| **Presentación de menús** | `Menu.Ejecutar` | Muestra un menú numerado y ejecuta la opción elegida hasta que se pulsa 0. | opciones → nada |
| | `MenuEntidad.DarDeAlta / Listar / Buscar / Modificar` | Pantallas comunes a personas y empresas (escritas una sola vez). | — |
| | `MenuEntidad.Localizar` | Pide un Id o texto y devuelve un único registro, o `null` si no existe. La usan modificar, baja y relaciones. | función de búsqueda → `T?` |
| | `MenuEntidad.MostrarFicha / MostrarTabla` | Presentan uno o varios registros. | registro(s) → nada |
| | `MenuPersonas.DarDeBaja`, `MenuEmpresas.DarDeBaja / Recuperar`, `MenuRelaciones.*` | Pantallas propias de cada bloque. | — |
| **Lectura y validación** | `Pantalla.LeerEntero` | Repite la pregunta hasta recibir un número. | mensaje → `int` |
| | `Pantalla.LeerOpcion` | **Pide una opción válida** de un menú; la usan todos los menús. | mensaje, opciones válidas → `int` |
| | `Pantalla.LeerConfirmacion` | Solo acepta S/SI/SÍ o N/NO. | pregunta → `bool` |
| | `Pantalla.LeerValorValido` | Pide un dato, lo normaliza y lo valida hasta que es correcto. | mensaje, normalizar, validar → `string` |
| | `Validador.ValidarTelefono / ValidarCorreo / ValidarObligatorio`, `ValidadorEmpresa.ValidarCif`… | Comprueban el formato de un dato. No leen ni escriben. | valor → `string?` (error o `null`) |
| | `Pantalla.MostrarExito / MostrarError / MostrarAviso` | Mensajes `[OK]`, `[ERROR]`, `[AVISO]` con color. | mensaje → nada |
| **Operaciones de gestión** | `ServicioPersonas.DarDeAlta / Modificar / Eliminar` | Validan y guardan personas (correo único, empresa activa). | `Persona` → `Resultado` |
| | `ServicioPersonas.AsignarEmpresa / Desvincular` | Gestionan la relación persona → empresa. | Ids → `Resultado` |
| | `ServicioEmpresas.DarDeAlta / Modificar / DarDeBaja / Recuperar` | Validan y guardan empresas; la baja se bloquea si tiene personas. | `Empresa` o Id → `Resultado` |
| | `Busqueda.PorCodigoOTexto` | **Búsqueda reutilizable por Id o texto**, común a personas y empresas. | criterio, prefijo, buscar por Id, buscar por texto → `List<T>` |
| | `CodigoId.Formatear / IntentarLeer` | Convierte entre el Id numérico y el código `P001`/`E001`. | — |
| **Datos** | `Repositorio*.Agregar / Actualizar / Eliminar / ObtenerPorId` | Guardan y recuperan registros. | — |

### Criterios aplicados

- **Sin variables globales:** `Program` crea los objetos como variables locales y se los pasa por constructor a quien los necesita.
- **Parámetros y retornos coherentes:** las validaciones devuelven el error (`string?`, `null` = correcto); las operaciones devuelven
  `Resultado` (correcto o lista de errores); las búsquedas devuelven `List<T>` (vacía si no hay nada); las lecturas devuelven el valor ya validado.
- **Sin duplicados:** `LeerOpcion` para cualquier menú, `Busqueda.PorCodigoOTexto` para buscar por Id o texto, `MenuEntidad<T>` para las pantallas comunes.

## Modelo de datos (si se pasa a base de datos)

> Esquema orientativo. Se ajustará cuando se conozca el motor de base de datos; la sintaxis del
> autoincremental cambia según el motor (`AUTOINCREMENT` en SQLite, `IDENTITY` en SQL Server,
> `AUTO_INCREMENT` en MySQL).

### Relación

```
┌──────────────────────┐           ┌──────────────────────┐
│       EMPRESAS       │           │       PERSONAS       │
├──────────────────────┤           ├──────────────────────┤
│ PK IdEmpresa         │ 1       N │ PK IdPersona         │
│    NombreComercial   │───────────│ FK IdEmpresa (NULL)  │
│    Cif     (único)   │           │    Nombre            │
│    Telefono          │           │    Apellidos         │
│    Correo            │           │    Telefono          │
│    Direccion         │           │    Correo  (único)   │
└──────────────────────┘           │    Cargo             │
                                   └──────────────────────┘
```

- **Una empresa tiene varias personas (1:N)** y una persona pertenece como máximo a una empresa.
- `Personas.IdEmpresa` admite `NULL`: una persona puede no tener empresa asignada.
- Las empresas no se borran: `Estado = 0` las oculta y `Estado = 1` las recupera.
- Los Id son enteros autoincrementales generados por la base de datos; `P001` / `E001` es solo cómo se muestran.

### Tablas

| Tabla | Campo | Tipo | Restricciones |
|---|---|---|---|
| Empresas | IdEmpresa | entero | PK, autoincremental |
| | NombreComercial | texto(100) | obligatorio |
| | Cif | texto(9) | obligatorio, único |
| | Telefono | texto(16) | obligatorio |
| | Correo | texto(100) | obligatorio |
| | Direccion | texto(150) | obligatorio |
| | Estado | entero | 1 = activa, 0 = baja (por defecto 1) |
| Personas | IdPersona | entero | PK, autoincremental |
| | Nombre | texto(50) | obligatorio |
| | Apellidos | texto(100) | obligatorio |
| | Telefono | texto(16) | obligatorio |
| | Correo | texto(100) | obligatorio, único |
| | Cargo | texto(50) | opcional |
| | IdEmpresa | entero | FK → Empresas.IdEmpresa, admite NULL |

### Script orientativo

```sql
CREATE TABLE Empresas (
    IdEmpresa       INTEGER PRIMARY KEY,      -- autoincremental
    NombreComercial VARCHAR(100) NOT NULL,
    Cif             VARCHAR(9)   NOT NULL UNIQUE,
    Telefono        VARCHAR(16)  NOT NULL,
    Correo          VARCHAR(100) NOT NULL,
    Direccion       VARCHAR(150) NOT NULL,
    Estado          INTEGER      NOT NULL DEFAULT 1   -- 1 = activa, 0 = baja
);

CREATE TABLE Personas (
    IdPersona  INTEGER PRIMARY KEY,           -- autoincremental
    Nombre     VARCHAR(50)  NOT NULL,
    Apellidos  VARCHAR(100) NOT NULL,
    Telefono   VARCHAR(16)  NOT NULL,
    Correo     VARCHAR(100) NOT NULL UNIQUE,
    Cargo      VARCHAR(50)  NULL,
    IdEmpresa  INTEGER      NULL,
    FOREIGN KEY (IdEmpresa) REFERENCES Empresas(IdEmpresa) ON DELETE RESTRICT
);

-- Consulta final del proyecto: qué personas pertenecen a cada empresa
SELECT e.NombreComercial, p.Nombre, p.Apellidos, p.Cargo
FROM Empresas e
LEFT JOIN Personas p ON p.IdEmpresa = e.IdEmpresa
WHERE e.Estado = 1
ORDER BY e.NombreComercial, p.Apellidos, p.Nombre;
```

### Cómo pasar a base de datos

Solo cambia la capa `Infrastructure`: se añade un almacén o unos repositorios de BD que implementen
`IRepositorioPersonas` e `IRepositorioEmpresas` (o `IAlmacenAgenda`) y se cambia `CrearAlmacen` en `Program.cs`.
`AgendaService` y `ConsoleUI` no se tocan. El JSON actual ya guarda las mismas columnas que tendrían las tablas.

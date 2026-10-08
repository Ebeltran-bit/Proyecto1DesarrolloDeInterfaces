# Agenda de la consultora — Fases 1 y 2: personas y empresas

Aplicación de consola en C# para dar de alta, listar, buscar, modificar y dar de baja personas y empresas,
con un menú principal y un submenú para cada una.

## Cómo ejecutarla

**Desde la terminal de VS Code**, en la carpeta `Proyecto1`:

```bash
dotnet run
```

Se recomienda `dotnet run` en la terminal integrada: si se lanza con F5, la configuración de depuración
debe usar `"console": "integratedTerminal"`, porque la consola de depuración de VS Code no permite escribir.

Requisitos: .NET SDK 10 (`dotnet --version`).

## Estructura

El código está organizado en capas. Cada una solo conoce a la de debajo:

```
Interfaz  →  Servicios  →  Datos
   (consola)   (reglas)      (almacenamiento)
        \          |          /
         └──── Modelos ──────┘
```

```
Proyecto1/
├── Proyecto1.csproj
├── Program.cs                        Crea las piezas, las conecta y arranca el menú
├── Modelos/
│   └── Persona.cs                    Datos de una persona
├── Datos/
│   ├── IRepositorioPersonas.cs       Contrato de almacenamiento
│   ├── RepositorioPersonasMemoria.cs Implementación con List<Persona>
│   └── DatosDeEjemplo.cs             4 personas para la demo
├── Servicios/
│   ├── ServicioPersonas.cs           Alta, búsqueda, modificación, baja, correo único
│   ├── ValidadorPersona.cs           Formato y normalización de cada dato
│   └── Resultado.cs                  Éxito o lista de errores de una operación
├── Interfaz/
│   ├── Menu.cs                       Menú numerado reutilizable (+ OpcionMenu)
│   ├── MenuPersonas.cs               Pantallas de alta, listado, búsqueda, modificación y baja
│   ├── Campo.cs                      Descripción de cada dato (etiqueta, validación, ancho…)
│   ├── Tabla.cs                      Dibuja cualquier lista como tabla alineada
│   └── Pantalla.cs                   Lectura validada y mensajes [OK] / [ERROR] / [AVISO]
└── Utilidades/
    ├── CodigoId.cs                   Id numérico ⇄ código P001
    └── Texto.cs                      Búsqueda sin mayúsculas ni tildes
```

## Decisiones de diseño

| Decisión | Justificación |
|---|---|
| **Capas separadas** (Modelos, Datos, Servicios, Interfaz) | Cada clase tiene una sola responsabilidad. Solo `Interfaz` usa `Console`, así que la lógica podrá reutilizarse cuando la aplicación tenga interfaz gráfica o web. |
| **Repositorio con interfaz** (`IRepositorioPersonas`) | Servicios y menús no saben dónde se guardan los datos. En la Fase 2 se añade `RepositorioPersonasBD` y solo cambia una línea de `Program.cs`. |
| **El repositorio en memoria trabaja con copias** | Se comporta como una base de datos: modificar un objeto no guarda nada hasta llamar a `Actualizar`. Así el código ya está escrito como funcionará con BD. |
| **Id `int` interno, código `P001` visible** | En una BD el Id será un entero autoincremental. `P001` es solo cómo se muestra y se busca. El contador solo avanza: un Id borrado nunca se reasigna, y no depende del nombre. |
| **Validación en el servicio** | Aunque la consola valida cada dato al escribirlo, el servicio vuelve a validar antes de guardar. Ninguna interfaz futura puede guardar datos incorrectos. |
| **`Resultado`** en vez de mensajes por pantalla | El servicio devuelve los errores y cada interfaz decide cómo mostrarlos. |
| **`Campo<T>`: cada dato definido una sola vez** | Alta, ficha, tabla y menú de modificar se generan a partir de la misma lista. Añadir un campo es añadir una línea en `MenuPersonas.CrearCampos()`. |
| **`Menu` y `Tabla` genéricos** | Sirven para el menú principal, el submenú de modificar y, en la Fase 2, para empresas. |
| Campo añadido: **Cargo** | En una agenda de consultora interesa saber el rol de cada contacto. Es opcional. |
| Empresa opcional | En la Fase 1 era texto libre; desde la Fase 2 es el `IdEmpresa` de una empresa activa. |
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

## Modelo de datos previsto (para cuando se pida base de datos)

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
    FOREIGN KEY (IdEmpresa) REFERENCES Empresas(IdEmpresa)
);

-- Consulta final del proyecto: qué personas pertenecen a cada empresa
SELECT e.NombreComercial, p.Nombre, p.Apellidos, p.Cargo
FROM Empresas e
LEFT JOIN Personas p ON p.IdEmpresa = e.IdEmpresa
WHERE e.Estado = 1
ORDER BY e.NombreComercial, p.Apellidos, p.Nombre;
```

### Cambios en el código al pasar a base de datos

Solo se añaden `RepositorioPersonasBD` y `RepositorioEmpresasBD` (implementan las mismas interfaces) y se cambian
las dos líneas de `Program.cs` que crean los repositorios.

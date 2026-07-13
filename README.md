# Sistema de Gestión de Tareas - Microservicios Backend (.NET 8)

Este repositorio contiene el backend del sistema de gestión de tareas, estructurado en una arquitectura de microservicios con **.NET 8** utilizando bases de datos **SQLite** independientes para cada servicio.

---

## 🛠️ Requisitos Previos

Antes de inicializar los servicios, asegúrate de tener instalado:
* **.NET 8.0 SDK** (o superior).
* **Visual Studio 2022** (con la carga de trabajo de desarrollo de ASP.NET y web instalada).

---

## 📂 Estructura de Proyectos

La solución contiene dos microservicios autónomos:

1. **UserManagementApi (Gestión de Usuarios)**
   * **Puerto local (HTTP):** `http://localhost:5001`
   * **Base de datos:** `users.db` (SQLite)
   * **Responsabilidad:** Gestionar el catálogo de usuarios, sus roles y datos de contacto.

2. **WorkItemsApi (Ítems de Trabajo)**
   * **Puerto local (HTTP):** `http://localhost:5002`
   * **Base de datos:** `workitems.db` (SQLite)
   * **Responsabilidad:** Gestionar los ítems de trabajo (tareas) y aplicar el motor de reglas de auto-asignación. Se comunica internamente con `UserManagementApi` a través de un cliente HTTP tipado.

---

## 🚀 Instrucciones de Inicialización

### Opción A: Desde Visual Studio 2022 (Recomendada)

Para ejecutar ambos microservicios de manera simultánea en Visual Studio:

1. Abre el archivo de solución `TaskManagementApi.sln` en Visual Studio 2022 (ubicado en `C:\Users\mmale\Downloads\TaskManagementApi\TaskManagementApi.sln`).
2. En el **Explorador de soluciones**, haz clic derecho sobre la solución (el nodo raíz `Solution 'TaskManagementApi'`) y selecciona **Propiedades** (Properties).
3. Ve a **Propiedades comunes** ➡️ **Proyecto de inicio** (Startup Project).
4. Selecciona la opción **Proyectos de inicio múltiples** (Multiple startup projects).
5. Configura la acción para ambos proyectos de la siguiente manera:
   * `UserManagementApi` ➡️ **Iniciar** (Start)
   * `WorkItemsApi` ➡️ **Iniciar** (Start)
6. Presiona **Aplicar** y luego **Aceptar**.
7. Presiona el botón **Iniciar** (o la tecla `F5`) en la barra de herramientas. Se abrirán dos ventanas de consola ejecutando ambos servicios.

### Opción B: Desde la Consola de Comandos (CLI)

Si prefieres ejecutar los proyectos utilizando la terminal de comandos, abre dos ventanas distintas de terminal en la raíz del proyecto (`C:\Users\mmale\Downloads\TaskManagementApi`) y ejecuta los siguientes comandos:

* **Ventana 1 (Gestión de Usuarios):**
  ```bash
  dotnet run --project UserManagementApi
  ```

* **Ventana 2 (Ítems de Trabajo):**
  ```bash
  dotnet run --project WorkItemsApi
  ```

---

## 💾 Persistencia y Semillado Inicial

Al iniciar los servicios por primera vez, el sistema creará automáticamente los archivos de base de datos SQLite (`users.db` y `workitems.db`) y semillará el escenario inicial:
* **Usuario A** (ID: `usr-a`) con 3 tareas asignadas.
* **Usuario B** (ID: `usr-b`) con 1 tarea asignada.
* Las bases de datos son persistentes entre reinicios. Si en algún momento necesitas restablecer todo al estado inicial, puedes presionar el botón **"Cargar Escenario Inicial"** en el panel de control del frontend.

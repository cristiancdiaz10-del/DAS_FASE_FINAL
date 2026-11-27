(() => {
    const api = "/api/materiales";

    async function solicitar(url, opciones = {}) {
        let respuesta;

        try {
            respuesta = await fetch(url, {
                credentials: "same-origin",
                ...opciones,
                headers: {
                    Accept: "application/json",
                    ...(opciones.body ? { "Content-Type": "application/json" } : {}),
                    ...opciones.headers
                }
            });
        } catch {
            throw new Error("No se pudo conectar con el servidor.");
        }

        if (!respuesta.ok) {
            let mensaje = `Error HTTP ${respuesta.status}.`;

            try {
                const cuerpo = await respuesta.json();
                const erroresValidacion = cuerpo.errors
                    ? Object.values(cuerpo.errors).flat().join(" ")
                    : "";

                mensaje =
                    cuerpo.mensaje ||
                    erroresValidacion ||
                    cuerpo.detail ||
                    cuerpo.title ||
                    mensaje;
            } catch {
                // Usa el mensaje HTTP si la respuesta no contiene JSON.
            }

            throw new Error(mensaje);
        }

        if (respuesta.status === 204) return null;

        const tipo = respuesta.headers.get("content-type") || "";
        return tipo.includes("application/json") ? respuesta.json() : null;
    }

    function mostrarError(contenedor, mensaje) {
        if (!contenedor) {
            alert(mensaje);
            return;
        }

        contenedor.textContent = mensaje;
        contenedor.hidden = false;
    }

    function crearCelda(fila, texto) {
        const celda = document.createElement("td");
        celda.textContent = texto ?? "";
        fila.appendChild(celda);
    }

    async function cargarMateriales(busqueda = "") {
        const tabla = document.getElementById("tablaMaterialesApi");
        const estado = document.getElementById("estadoMateriales");

        if (!tabla) return;

        if (estado) estado.textContent = "Cargando materiales...";

        try {
            const url = new URL(api, window.location.origin);
            if (busqueda) url.searchParams.set("busqueda", busqueda);

            const materiales = await solicitar(url);
            tabla.replaceChildren();

            if (!materiales || materiales.length === 0) {
                const fila = document.createElement("tr");
                const celda = document.createElement("td");
                celda.colSpan = 6;
                celda.textContent = "No hay materiales registrados.";
                fila.appendChild(celda);
                tabla.appendChild(fila);
            } else {
                materiales.forEach(material => {
                    const fila = document.createElement("tr");

                    crearCelda(fila, material.codigoMaterial);
                    crearCelda(fila, material.nombreMaterial);
                    crearCelda(fila, Number(material.cantidadDisponible).toFixed(2));
                    crearCelda(fila, material.unidadMedida);
                    crearCelda(fila, Number(material.costoUnitario).toFixed(2));

                    const acciones = document.createElement("td");

                    const editar = document.createElement("a");
                    editar.className = "link";
                    editar.textContent = "Editar";
                    editar.href = `/Inventario/EditarMaterial?id=${encodeURIComponent(material.idMaterial)}`;

                    const eliminar = document.createElement("button");
                    eliminar.type = "button";
                    eliminar.className = "danger";
                    eliminar.textContent = "Eliminar";
                    eliminar.addEventListener("click", async () => {
                        if (!confirm(`¿Eliminar el material ${material.codigoMaterial}?`)) return;

                        try {
                            await solicitar(`${api}/${material.idMaterial}`, {
                                method: "DELETE"
                            });
                            await cargarMateriales(
                                document.querySelector('[name="busqueda"]')?.value.trim() || ""
                            );
                        } catch (error) {
                            alert(error.message);
                        }
                    });

                    acciones.append(editar, document.createTextNode(" "), eliminar);
                    fila.appendChild(acciones);
                    tabla.appendChild(fila);
                });
            }

            if (estado) estado.textContent = "Materiales cargados.";
        } catch (error) {
            if (estado) estado.textContent = "No se pudieron cargar los materiales.";
            mostrarError(document.getElementById("errorMateriales"), error.message);
        }
    }

    document.addEventListener("DOMContentLoaded", () => {
        const tabla = document.getElementById("tablaMaterialesApi");
        if (tabla) {
            cargarMateriales();

            document.getElementById("formBuscarMateriales")
                ?.addEventListener("submit", event => {
                    event.preventDefault();
                    const busqueda =
                        document.querySelector('[name="busqueda"]')?.value.trim() || "";
                    cargarMateriales(busqueda);
                });
        }

        document.querySelectorAll("form[data-material-api]").forEach(formulario => {
            formulario.addEventListener("submit", async event => {
                event.preventDefault();

                const resumen =
                    formulario.querySelector(".validation-summary-errors");
                if (resumen) {
                    resumen.textContent = "";
                    resumen.hidden = true;
                }

                const datos = new FormData(formulario);
                const id = Number(datos.get("IdMaterial") || 0);

                const material = {
                    codigoMaterial: String(datos.get("CodigoMaterial") || "").trim(),
                    nombreMaterial: String(datos.get("NombreMaterial") || "").trim(),
                    cantidadDisponible: Number(datos.get("CantidadDisponible")),
                    unidadMedida: String(datos.get("UnidadMedida") || "").trim(),
                    costoUnitario: Number(datos.get("CostoUnitario")),
                    descripcion: String(datos.get("Descripcion") || "").trim() || null
                };

                if (id) material.idMaterial = id;

                const boton = formulario.querySelector('[type="submit"]');
                if (boton) boton.disabled = true;

                try {
                    await solicitar(id ? `${api}/${id}` : api, {
                        method: id ? "PUT" : "POST",
                        body: JSON.stringify(material)
                    });

                    window.location.href = "/Inventario/Index";
                } catch (error) {
                    mostrarError(resumen, error.message);
                } finally {
                    if (boton) boton.disabled = false;
                }
            });
        });
    });
})();
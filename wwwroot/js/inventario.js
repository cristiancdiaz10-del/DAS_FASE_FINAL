
(() => {
    "use strict";

    const api = "/api/materiales";
    const encabezadoAntiforgery = "RequestVerificationToken";

    async function leerError(respuesta) {
        let mensaje = `Error HTTP ${respuesta.status}.`;

        try {
            const cuerpo = await respuesta.json();

            const errores = cuerpo.errors
                ? Object.values(cuerpo.errors).flat().join(" ")
                : "";

            mensaje =
                cuerpo.mensaje ||
                errores ||
                cuerpo.detail ||
                cuerpo.title ||
                mensaje;
        } catch {
            // Conserva el mensaje HTTP si no existe una respuesta JSON.
        }

        return mensaje;
    }

    // Solicita un token nuevo al servidor y conserva la cookie antiforgery.
    async function obtenerTokenAntiforgery() {
        const respuesta = await fetch("/api/auth/antiforgery", {
            method: "GET",
            credentials: "same-origin",
            cache: "no-store",
            headers: {
                Accept: "application/json"
            }
        });

        if (!respuesta.ok) {
            throw new Error(await leerError(respuesta));
        }

        const datos = await respuesta.json();

        if (!datos.requestToken) {
            throw new Error(
                "El servidor no generó el token antifalsificación."
            );
        }

        return datos.requestToken;
    }

    async function renovarSesion(tokenAntiforgery) {
        const respuesta = await fetch("/api/auth/refresh", {
            method: "POST",
            credentials: "same-origin",
            headers: {
                Accept: "application/json",
                [encabezadoAntiforgery]: tokenAntiforgery
            }
        });

        return respuesta.ok;
    }

    async function solicitar(url, opciones = {}) {
        const metodo = String(
            opciones.method || "GET"
        ).toUpperCase();

        const requiereAntiforgery =
            !["GET", "HEAD", "OPTIONS", "TRACE"].includes(metodo);

        const headers = new Headers(opciones.headers || {});

        headers.set("Accept", "application/json");

        if (opciones.body && !headers.has("Content-Type")) {
            headers.set("Content-Type", "application/json");
        }

        let tokenAntiforgery = null;

        if (requiereAntiforgery) {
            tokenAntiforgery = await obtenerTokenAntiforgery();

            headers.set(
                encabezadoAntiforgery,
                tokenAntiforgery
            );
        }

        const ejecutar = () => fetch(url, {
            ...opciones,
            method: metodo,
            credentials: "same-origin",
            headers
        });

        let respuesta;

        try {
            respuesta = await ejecutar();

            // Si expiró el JWT, renueva mediante la sesión Identity.
            if (respuesta.status === 401) {
                if (!tokenAntiforgery) {
                    tokenAntiforgery =
                        await obtenerTokenAntiforgery();
                }

                const renovada =
                    await renovarSesion(tokenAntiforgery);

                if (renovada) {
                    respuesta = await ejecutar();
                }
            }
        } catch (error) {
            if (error instanceof Error &&
                error.message !== "Failed to fetch") {
                throw error;
            }

            throw new Error(
                "No se pudo conectar con el servidor."
            );
        }

        if (!respuesta.ok) {
            throw new Error(await leerError(respuesta));
        }

        if (respuesta.status === 204) {
            return null;
        }

        const tipo = respuesta.headers.get("content-type") || "";

        return tipo.includes("application/json")
            ? await respuesta.json()
            : null;
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

        const contenedorError =
            document.getElementById("errorMateriales");

        if (estado) {
            estado.textContent = "Cargando materiales...";
        }

        if (contenedorError) {
            contenedorError.textContent = "";
            contenedorError.hidden = true;
        }

        try {
            const url = new URL(api, window.location.origin);

            if (busqueda) {
                url.searchParams.set("busqueda", busqueda);
            }

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

                    crearCelda(
                        fila,
                        Number(material.cantidadDisponible).toFixed(2)
                    );

                    crearCelda(fila, material.unidadMedida);

                    crearCelda(
                        fila,
                        Number(material.costoUnitario).toFixed(2)
                    );

                    const acciones = document.createElement("td");

                    const editar = document.createElement("a");
                    editar.className = "link";
                    editar.textContent = "Editar";
                    editar.href =
                        `/Inventario/EditarMaterial?id=${encodeURIComponent(
                            material.idMaterial
                        )}`;

                    const eliminar = document.createElement("button");
                    eliminar.type = "button";
                    eliminar.className = "danger";
                    eliminar.textContent = "Eliminar";

                    eliminar.addEventListener("click", async () => {
                        if (!confirm(
                            `¿Eliminar el material ${material.codigoMaterial}?`
                        )) {
                            return;
                        }

                        try {
                            await solicitar(
                                `${api}/${material.idMaterial}`,
                                { method: "DELETE" }
                            );

                            await cargarMateriales(
                                document.querySelector(
                                    '[name="busqueda"]'
                                )?.value.trim() || ""
                            );
                        } catch (error) {
                            alert(error.message);
                        }
                    });

                    acciones.append(
                        editar,
                        document.createTextNode(" "),
                        eliminar
                    );

                    fila.appendChild(acciones);
                    tabla.appendChild(fila);
                });
            }

            if (estado) {
                estado.textContent = "Materiales cargados.";
            }

        } catch (error) {
            if (estado) {
                estado.textContent =
                    "No se pudieron cargar los materiales.";
            }

            mostrarError(contenedorError, error.message);
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
                        document.querySelector('[name="busqueda"]')
                            ?.value.trim() || "";

                    cargarMateriales(busqueda);
                });
        }

        document.querySelectorAll(
            "form[data-material-api]"
        ).forEach(formulario => {

            formulario.addEventListener("submit", async event => {
                event.preventDefault();

                const resumen = formulario.querySelector(
                    "[data-valmsg-summary='true']"
                );

                if (resumen) {
                    resumen.textContent = "";
                    resumen.hidden = true;
                }

                const datos = new FormData(formulario);
                const id = Number(datos.get("IdMaterial") || 0);

                const material = {
                    codigoMaterial: String(
                        datos.get("CodigoMaterial") || ""
                    ).trim(),

                    nombreMaterial: String(
                        datos.get("NombreMaterial") || ""
                    ).trim(),

                    cantidadDisponible: Number(
                        datos.get("CantidadDisponible")
                    ),

                    unidadMedida: String(
                        datos.get("UnidadMedida") || ""
                    ).trim(),

                    costoUnitario: Number(
                        datos.get("CostoUnitario")
                    ),

                    descripcion: String(
                        datos.get("Descripcion") || ""
                    ).trim() || null
                };

                if (id) {
                    material.idMaterial = id;
                }

                const boton = formulario.querySelector(
                    '[type="submit"]'
                );

                if (boton) boton.disabled = true;

                try {
                    await solicitar(
                        id ? `${api}/${id}` : api,
                        {
                            method: id ? "PUT" : "POST",
                            body: JSON.stringify(material)
                        }
                    );

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

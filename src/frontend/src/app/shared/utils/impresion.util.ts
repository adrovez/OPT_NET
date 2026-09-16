/**
 * Imprime lo que haya dentro del `MatDialog` abierto en este momento — reemplaza al
 * `window.print()` liso del legacy (`TicketOT.js`), que dependía de clonar nodos a mano.
 *
 * Marca el `<body>` con `opt-imprimiendo` mientras dura la impresión: la regla `@media print`
 * de `styles.scss` global usa esa clase para ocultar todo menos el diálogo (un `@media print`
 * encapsulado por componente no puede ocultar el resto de la app), y la retira al terminar.
 *
 * Compartida entre todo diálogo que imprime un ticket (`OrdenCreadaDialog`,
 * `ImprimirTicketDialog`) para no duplicar la lógica de limpieza.
 */
export function imprimirConClaseBody(): void {
  const body = document.body;
  body.classList.add('opt-imprimiendo');

  const limpiar = () => {
    body.classList.remove('opt-imprimiendo');
    window.removeEventListener('afterprint', limpiar);
  };
  // `afterprint` no dispara en todos los navegadores si el usuario cancela desde el diálogo
  // nativo; el `finally` del print síncrono es la red de seguridad.
  window.addEventListener('afterprint', limpiar);

  try {
    window.print();
  } finally {
    limpiar();
  }
}

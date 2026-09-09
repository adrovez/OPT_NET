import { Component } from '@angular/core';

import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { PageHeader } from '../../../../shared/components/page-header/page-header';

@Component({
  selector: 'app-inventario-list',
  imports: [EmptyState, PageHeader],
  templateUrl: './inventario-list.html',
  styleUrl: './inventario-list.scss',
})
export class InventarioList {}

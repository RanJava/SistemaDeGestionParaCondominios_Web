import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app';
import { registerLocaleData } from '@angular/common';
import localeEs from '@angular/common/locales/es';

// Registra el locale español para que los pipes de fecha
// muestren los meses en español
registerLocaleData(localeEs);

bootstrapApplication(App, appConfig).catch(err => console.error(err));
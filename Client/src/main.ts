import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { AppComponent } from './app/app.component';
import config from 'devextreme/core/config';

config({ licenseKey: 'ewogICJmb3JtYXQiOiAxLAogICJjdXN0b21lcklkIjogIjhmZGNkODRmLTkzY2EtNGZhNS1iMTViLWVmOWZmZmVmZTY5NiIsCiAgIm1heFZlcnNpb25BbGxvd2VkIjogMjUxCn0=.L6Wp9xZhwCcGPbMHTv3zIqTHDKNxB8N9M1aERfm6ULM6bjM7L2PfHq2BbRtjpgDxBI7XRkdIelfiDq9hZFm9EMxxmTtY0YB4eKIg2n8KiuH4brewgttYf+xeMQ8lSkgz5jiwHQ==' });

bootstrapApplication(AppComponent, appConfig)
  .catch((err) => console.error(err));

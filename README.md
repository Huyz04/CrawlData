# CrawlData
_Đây là challenge tiếp theo mà TTH và Trần Ngọc Quyên sẽ làm_

## Nội dung:

Create a news reading website using Angular + Bootstrap or TailwindCSS. Articles at the bottom will load in a lazy load more format. You can use ng-zorro for coding frontend more quickly


The backend uses .net. We need Swagger for documentation, and a cronjob to crawl new articles from https://tuoitre.vn/.


Design the database to appropriately store categories and articles.


The cronjob runs every 30 minutes and does not crawl old data.


The backend and frontend are divided into two separate projects.


The backend uses logs to record requests or exceptions when errors occur in the code.


Logs are stored with the following structure: log folder => log file named log_10102022 (10102022 represents the current date, all errors from this day are stored in this file).


Use PostgreSQL for the database, with a clear project structure including models, providers, controllers, services, etc.


Configure two environment files: one for local development and one for production.


The cronjob uses a simple background worker.


Clean code and handle for exceptions


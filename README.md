# OrderInventory

Ürün stoku ve sipariş yönetimi için geliştirilmiş ASP.NET Core Web API. PostgreSQL, EF Core ve Clean Architecture katmanlarını kullanır.

## İçindekiler

- [Çalıştırma](#çalıştırma)
- [Testler](#testler)
- [Mimari kararlar](#mimari-kararlar)
- [Bilinen eksikler](#bilinen-eksikler)

## Çalıştırma

### Gereksinimler

- **.NET 10 SDK** — projeyi yerel olarak derlemek ve unit testleri çalıştırmak için.
- **Docker Engine/Desktop ve Docker Compose v2** — API'yi, PostgreSQL'i ve PostgreSQL entegrasyon testlerini çalıştırmak için.
- Varsayılan olarak **8080** (API) ve **5433** (PostgreSQL host portu) portlarının boş olması gerekir. Portlar `.env` dosyasından değiştirilebilir.

### Docker ile başlatma

1. `.env` dosyanız yoksa örnek dosyadan oluşturun:

   ```bash
   if [ ! -f .env ]; then cp .env.example .env; fi
   ```

2. `.env` içindeki `POSTGRES_PASSWORD` değerini yerel kullanım için değiştirin. `.env` dosyası Git'e ve Docker build context'ine dahil edilmez.

3. PostgreSQL'i başlatın:

   ```bash
   docker compose up -d db
   ```

4. EF Core migration'larını uygulayın:

   ```bash
   docker compose run --build --rm migrate
   ```

5. API'yi derleyip başlatın:

   ```bash
   docker compose up --build -d api
   ```

Compose, API'yi `db` servis adına bağlar ve PostgreSQL'in health check'inin başarılı olmasını bekler. Migration'lar uygulama başlarken otomatik çalışmaz; yeni migration eklendiğinde 4. adımı tekrar çalıştırın.

### Adresler ve servis yönetimi

- **Scalar API arayüzü:** [http://localhost:8080/scalar](http://localhost:8080/scalar)
- **OpenAPI belgesi:** [http://localhost:8080/openapi/v1.json](http://localhost:8080/openapi/v1.json)

Servis durumunu ve API loglarını görmek için:

```bash
docker compose ps
docker compose logs -f api
```

Servisleri durdurmak için:

```bash
docker compose down
```

Bu komut PostgreSQL verilerini volume'da tutar. Yerel veritabanını da silmek için `docker compose down -v` kullanılabilir; bu komut volume'daki verileri kalıcı olarak siler.

## Testler

Unit testleri çalıştırmak için:

```bash
dotnet test test/OrderInventory.Tests/OrderInventory.Tests.csproj
```

PostgreSQL concurrency entegrasyon testi Testcontainers ile geçici bir PostgreSQL container'ı başlatır; bu nedenle Docker çalışıyor olmalıdır.

## Mimari kararlar

- **Clean Architecture:** Domain, Application, Infrastructure ve API katmanları ayrıdır. İş kuralları Application/Domain katmanlarında, veritabanı ve HTTP ayrıntıları ise ilgili altyapı katmanlarında tutulur; bu ayrım bağımlılıkları sınırlar ve test edilebilirliği artırır.
- **Generic Repository kullanılmaması:** `IProductRepository` ve `IOrderRepository`, domain'e özgü sorguları açıkça ifade eder. Genel bir CRUD repository'si çoğu zaman EF Core davranışını tekrarlar ve gereken sorgu özelliklerini gizler.
- ** Unit of Work:** `IUnitOfWork` yalnızca `SaveChangesAsync` çağrısını temsil eder. Repository'lerin aynı scoped `AppDbContext` üzerindeki değişiklikleri tek noktadan kaydedilir; change tracking ve transaction yönetimi EF Core'a bırakılır.
- **Stok ve siparişin birlikte kaydedilmesi:** Sipariş oluşturulurken stok düşürme ve sipariş ekleme aynı `SaveChangesAsync` çağrısında kaydedilir. EF Core'un ilişkisel sağlayıcısı bu değişiklikleri tek transaction içinde uygular; işlem başarısız olursa birlikte geri alınırlar.
- **Ürün stoğunda concurrency kontrolü:** PostgreSQL'in her satır güncellemesinde değiştirdiği `xmin` sistem sütunu, ürünler için optimistic concurrency token olarak kullanılır. Eski stok verisine dayalı güncelleme `DbUpdateConcurrencyException` üretir; API bunu `409 Conflict` olarak yanıtlar.
- **Sipariş anındaki fiyat:** `OrderItem.UnitPrice` alanı, ürünün ileride değişebilecek fiyatından bağımsız olarak sipariş fiyatını saklamak için kullanılır. Ancak mevcut uygulama bu değeri istemci DTO'sundan kopyalar; fiyatı sunucu tarafında ürün kaydından hesaplamaz veya doğrulamaz.

## Bilinen eksikler

- Kimlik doğrulama (authentication) ve yetkilendirme (authorization) yok.
- Ürün fiyatı ve sipariş toplamı sunucu tarafında hesaplanıp doğrulanmalı; şu anda fiyat istemciden geliyor.
- Concurrency koruması ürün satırlarında mevcut; otomatik retry politikası ve tüm iş akışlarını kapsayan daha geniş concurrency testleri yok.
- Redis cache ve RabbitMQ mesajlaşması eklenmedi.
- Entegrasyon test kapsamı sınırlı; PostgreSQL testi şu anda eşzamanlı son stok siparişi senaryosuna odaklanıyor. API uçtan uca senaryoları da artırılmalı.
- `OrderService.DeleteAsync` henüz uygulanmadı.
- Docker Compose ayarları yerel geliştirme içindir. Production için TLS, secret yönetimi ve merkezi loglama/izleme ayrıca yapılandırılmalı.

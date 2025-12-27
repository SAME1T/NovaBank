-- Veritabanını Sıfırlama Scripti
-- Tüm verileri siler
-- DİKKAT: Bu script tüm verileri siler!
-- Uygulama başlatıldığında admin kullanıcısı ve sistem hesapları otomatik olarak oluşturulacak

-- Foreign key constraint'leri geçici olarak devre dışı bırak
SET session_replication_role = 'replica';

-- Tüm tabloları temizle (foreign key sırasına göre)
TRUNCATE TABLE "bank_transactions" CASCADE;
TRUNCATE TABLE "bank_transfers" CASCADE;
TRUNCATE TABLE "bank_cards" CASCADE;
TRUNCATE TABLE "bank_payment_orders" CASCADE;
TRUNCATE TABLE "bank_loans" CASCADE;
TRUNCATE TABLE "bank_credit_card_applications" CASCADE;
TRUNCATE TABLE "bank_accounts" CASCADE;
TRUNCATE TABLE "approval_workflows" CASCADE;
TRUNCATE TABLE "transaction_limits" CASCADE;
TRUNCATE TABLE "commissions" CASCADE;
TRUNCATE TABLE "kyc_verifications" CASCADE;
TRUNCATE TABLE "bill_payments" CASCADE;
TRUNCATE TABLE "notifications" CASCADE;
TRUNCATE TABLE "notification_preferences" CASCADE;
TRUNCATE TABLE "exchange_rates" CASCADE;
TRUNCATE TABLE "audit_logs" CASCADE;
TRUNCATE TABLE "password_reset_tokens" CASCADE;
TRUNCATE TABLE "bank_customers" CASCADE;
TRUNCATE TABLE "branches" CASCADE;
TRUNCATE TABLE "bill_institutions" CASCADE;

-- Foreign key constraint'leri tekrar aktif et
SET session_replication_role = 'origin';

-- NOT: Admin kullanıcısı ve sistem hesapları uygulama başlatıldığında 
-- Program.cs içindeki seed işlemleri tarafından otomatik olarak oluşturulacak.
-- Admin bilgileri:
-- TC: 11111111111
-- Şifre: Admin123!


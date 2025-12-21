-- NovaBank 7.8 Fix Pack - Index Çakışması Düzeltme
-- Eğer migration sırasında "index already exists" hatası alırsanız:
-- Bu SQL'i pgAdmin Query Tool'da çalıştırın, sonra migration'ı tekrar çalıştırın.

DROP INDEX IF EXISTS "IX_bank_password_reset_tokens_CustomerId_IsUsed_ExpiresAt";
DROP INDEX IF EXISTS "IX_bank_transfers_ReversalOfTransferId";
DROP INDEX IF EXISTS "IX_bank_transfers_ReversedByTransferId";

-- Kontrol için:
SELECT indexname 
FROM pg_indexes 
WHERE schemaname='public' 
  AND tablename IN ('bank_password_reset_tokens', 'bank_transfers')
ORDER BY tablename, indexname;


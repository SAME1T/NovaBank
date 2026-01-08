namespace NovaBank.Core.Enums;

/// <summary>
/// Kullanıcı rolleri.
/// </summary>
public enum UserRole
{
    Customer = 0,
    Admin = 1,
    Manager = 2,
    /// <summary>
    /// Şube Bankacı Yönetimi - Admin'in tüm işlemlerini yapabilir (hesap/kullanıcı silme hariç)
    /// Kayıt ekranından kaydolamaz, sadece Admin tarafından oluşturulabilir.
    /// </summary>
    BranchManager = 3
}


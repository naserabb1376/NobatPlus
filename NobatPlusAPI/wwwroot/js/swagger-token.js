(function waitForUI() {
    if (!window.ui) {
        console.log("⏳ Swagger UI is loading.");
        setTimeout(waitForUI, 500);
        return;
    }

    console.log("✅ Swagger UI Ready");
    const ui = window.ui;
    const loginPath = "/authentication/Authenticate"; // مسیر لاگین شما

    // 1) اگر قبلاً ذخیره شده، توکن را تزریق کن
    const saved = localStorage.getItem("swagger_jwt");
    if (saved) {
        try {
            // اگر نوع‌تون http bearer هست معمولاً بدون پیشوند کافی است:
            ui.preauthorizeApiKey("Bearer", saved);
            // در صورت نیاز: ui.preauthorizeApiKey("Bearer", "Bearer " + saved);
        } catch (e) {
            console.warn("Preauthorize from storage failed:", e);
        }
    }

    // 2) پاسخ‌های Swagger را مانیتور کن و اگر لاگین بود، توکن را از res.text بخوان
    const responseInterceptor = (res) => {
        try {
            if (res?.url?.includes(loginPath) && res.status === 200 && res.text) {
                const data = JSON.parse(res.text);

                // مسیرهای احتمالی برای استخراج توکن (هر کدوم که در API شما هست)
                const token =
                    data?.result?.accessToken ??
                    data?.Data?.AccessToken ??
                    data?.accessToken ??
                    data?.token;

                if (token) {
                    console.log("✅ Token received.");
                    localStorage.setItem("swagger_jwt", token);

                    // برای Http Bearer معمولاً بدون پیشوند:
                    ui.preauthorizeApiKey("Bearer", token);
                    // اگر کار نکرد، این را جایگزین کن:
                    // ui.preauthorizeApiKey("Bearer", "Bearer " + token);
                } else {
                    console.warn("⚠️ Token not found in response payload:", data);
                }
            }
        } catch (err) {
            console.error("Response interceptor error:", err, res);
        }
        return res; // خیلی مهم: همیشه res را برگردان
    };

    const config = ui.getConfigs?.();
    if (config) {
        config.responseInterceptor = responseInterceptor;
    } else {
        console.warn("⚠️ Config not available.");
    }
})();




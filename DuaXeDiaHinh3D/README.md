# ĐUA XE ĐỊA HÌNH 3D

Đồ án game đua xe địa hình 3D hoàn chỉnh, dựng 100% bằng Unity + C#. Toàn bộ mô hình xe,
nhân vật, địa hình, giao diện, texture và âm thanh đều được sinh bằng code lúc chạy,
không phụ thuộc Asset Store hay tài nguyên có bản quyền.

- **Unity:** 6000.3.29f1 (Unity 6.3), Built-in Render Pipeline
- **Ngôn ngữ:** C#
- **Nền tảng build:** Windows 64-bit
- **Thư mục dự án:** `C:\Users\XuanNguyen PC\Desktop\DO AN GAME\DuaXeDiaHinh3D`

---

## 1. Cách mở và chạy

1. Mở Unity Hub → **Add project from disk** → chọn thư mục `DuaXeDiaHinh3D`.
2. Mở project bằng Unity **6000.3.29f1**.
3. Trong Unity, chọn menu **DuaXe3D → 1. Thiết lập dự án (6 Scene)** để tạo lại 6 scene
   và cấu hình Build Settings (lần đầu đã tạo sẵn).
4. Mở scene `Assets/01_Canh/01_MenuChinh.unity` và bấm **Play**.
5. Build: menu **DuaXe3D → 2. Build Windows**. File chạy nằm ở `Build/DuaXeDiaHinh3D.exe`.
6. Build + tự kiểm thử + chụp ảnh: **DuaXe3D → 3. Build + Auto Test**.
   Ảnh được lưu vào thư mục `Screenshots/`.

### Chạy tự kiểm thử bằng dòng lệnh

```powershell
Build\DuaXeDiaHinh3D.exe -autotest -screenshotdir "Screenshots" `
  -screen-width 1600 -screen-height 900 -screen-fullscreen 0
```

Chế độ này tự đi qua 6 màn hình, chụp ảnh từng màn rồi thoát.

---

## 2. Điều khiển

| Phím | Chức năng |
|------|-----------|
| **W / ↑** | Tăng tốc |
| **S / ↓** | Phanh / lùi |
| **A / ←** | Đánh lái trái |
| **D / →** | Đánh lái phải |
| **Space** | Phanh tay / drift |
| **Shift** | Nitro (tăng tốc tức thời) |
| **R** | Đưa xe về lại đường đua |
| **Esc** | Tạm dừng (trong đua) / quay lại (trong menu) |
| **Enter** | Bắt đầu / tiếp tục |
| **Chuột trái + kéo** | Xoay camera ở màn chọn địa hình |

---

## 3. Luồng 6 màn hình

```
01_MenuChinh → 02_ChonXe → 03_ChonNhanVat → 04_ChonDiaHinh → 05_DuaXe → 06_KetQua
       ↑                                                                   |
       └────────────────────────── ĐUA LẠI / VỀ MENU ─────────────────────┘
```

| Scene | Nội dung |
|-------|----------|
| `01_MenuChinh` | Menu chính, xe trưng bày xoay trên bệ 3D, chọn nhanh xe/tay đua/địa hình, bật tắt nhạc & SFX, kỷ lục |
| `02_ChonXe` | 3 mẫu xe: Jeep địa hình, Buggy bão tố, Monster Truck; xem chỉ số và mô hình 3D xoay |
| `03_ChonNhanVat` | 3 tay đua: Bão Lửa, Sóng Thần, Bóng Đêm; mô hình nhân vật 3D + chỉ số |
| `04_ChonDiaHinh` | 4 địa hình: Sa mạc, Rừng già, Đồi núi, Đường bùn; xem terrain 3D thu nhỏ, độ khó, kỷ lục |
| `05_DuaXe` | Đua thực tế: đếm ngược, HUD, đồng hồ tốc độ, bản đồ nhỏ, nitro, drift, AI, về đích |
| `06_KetQua` | Bảng xếp hạng Top 1/2/3, thời gian, kỷ lục mới, bệ podium 3D, chơi lại |

Lựa chọn được lưu bằng `PlayerPrefs` và tự động khôi phục ở lần chơi sau.

---

## 4. Cấu trúc thư mục

```
Assets/
├── 01_Canh/                     # 6 scene
│   ├── 01_MenuChinh.unity
│   ├── 02_ChonXe.unity
│   ├── 03_ChonNhanVat.unity
│   ├── 04_ChonDiaHinh.unity
│   ├── 05_DuaXe.unity
│   └── 06_KetQua.unity
├── 02_MaNguon/
│   ├── AmThanh/                 # SynthAudio, AudioManager, EngineAudio
│   ├── DiaHinh/                 # TerrainBuilder, RoadPath, RoadMeshBuilder,
│   │                            # EnvironmentBuilder, PropBuilder, ThemeLibrary
│   ├── GiaoDien/                # UITheme, UIKit, MenuKit, RaceHUD, CameraPostFX,
│   │                            # 6 scene bootstrap, MainMenuScene...
│   ├── NhanVat/                 # CharacterFactory
│   ├── QuanLy/                  # GameSession, SceneFlow, RaceManager, AutoTestRunner
│   └── Xe/                      # CarLibrary, CarFactory, ArcadeCarController,
│                                # RacerInfo, AIDriver, ChaseCamera, NitroPickup
├── 03_Xe/                       # (dành cho model/prefab xe bổ sung)
├── 04_NhanVat/                  # (dành cho model nhân vật bổ sung)
├── 05_DiaHinh/                  # (dành cho terrain asset bổ sung)
├── 06_AmThanh/                  # (dành cho audio clip bổ sung)
├── 07_VatLieu/                  # MaterialLibrary, MeshKit, shader Mountain/PostFX
├── 08_Prefab/                   # (dành cho prefab bổ sung)
├── 09_HinhAnh/                  # Font OFL, hình ảnh
└── Editor/                      # ProjectBuilder: tạo scene + build
```

---

## 5. Đối chiếu yêu cầu đồ án

| Yêu cầu | Cài đặt |
|---------|---------|
| 6 scene đúng tên | `Assets/01_Canh/*.unity`, tạo bởi `ProjectBuilder` |
| 3 mẫu xe | `CarLibrary.All` + `CarFactory` (Jeep, Buggy, Monster Truck) |
| 3 tay đua | `CharacterLibrary.All` + `CharacterFactory` |
| 4 địa hình | `ThemeLibrary.All` (Sa mạc, Rừng già, Đồi núi, Đường bùn) |
| Địa hình có đồi, ≥3 dốc, ≥2 hố đất, ≥1 cua gắt | `TerrainBuilder` (heightmap + carve đường), `TrackLayout.Build`, `PropBuilder` |
| Đường đua uốn lượn | `RoadPath` + `RoadMeshBuilder` (~1.3 km, nhiều cua) |
| Xe dùng Rigidbody, không rơi xuyên địa hình | `CarFactory.SpawnRaceCar`, `ArcadeCarController`, `MeshCollider` đường + TerrainCollider |
| Điều khiển W/S/A/D | `ArcadeCarController.Update` |
| Camera bám xe mượt, nhìn theo xe | `ChaseCamera` (SmoothDamp + raycast chống xuyên địa hình) |
| Nhạc nền + tiếng động cơ | `SynthAudio`, `AudioManager`, `EngineAudio` (tổng hợp bằng code) |
| Thông báo "Bạn đã chiến thắng" → màn kết quả | `RaceManager.FinishPlayer`, `RaceHUD.ShowFinish`, `ResultsScene` |
| HUD: tên xe, thời gian, tốc độ | `RaceHUD` (nhãn tên xe + tay đua cố định, vị trí, đồng hồ, tốc độ, nitro, minimap) |
| Lưu lựa chọn bằng PlayerPrefs | `GameSession` |
| Drift | Phanh tay + `ArcadeCarController` xử lý trượt |
| Nitro | `Nitro01`, phím Shift, bình nitro trên đường (`NitroPickup`) |
| ≥1 AI | 3 AI trong `RaceScene`, điều khiển bởi `AIDriver` |
| Xếp hạng Top 1/2/3 | `RaceManager.ComputeRanks`, podium + bảng hạng trong `ResultsScene` |
| Giao diện 3D/UI đẹp, miễn phí | UI procedural + font OFL + studio 3D + hậu kỳ `CameraPostFX` |

---

## 6. Thư viện / asset miễn phí nên bổ sung (tùy chọn)

Bản hiện tại **không cần** thư viện ngoài. Nếu muốn nâng cấp hình ảnh/âm thanh thực tế hơn,
có thể thêm các gói miễn phí sau (kiểm tra license trước khi dùng trong đồ án):

### UI & tiện ích
- **TextMeshPro** – có sẵn trong Unity/uGUI 2.0, chữ sắc nét, hiệu ứng chữ tốt hơn `Text`.
- **DOTween (Demigiant)** – miễn phí, animation UI mượt (fade, popup, đếm số).
- **Unity UI Samples** – mẫu layout, button, slider miễn phí từ Unity.
- **Kenney UI Pack** (CC0) – icon, khung, button phẳng chất lượng cao.

### Camera & hậu kỳ
- **Cinemachine** (`com.unity.cinemachine`) – camera theo xe, rung, blend giữa menu/race.
- **Post Processing Stack v2** (`com.unity.postprocessing`) – bloom, color grading, DOF, vignette.
- **AllSky Free** – bộ skybox miễn phí.
- **Poly Haven HDRI** (CC0) – môi trường ánh sáng thực tế.

### Địa hình & thiên nhiên
- **Terrain Tools** (`com.unity.terrain-tools`) – công cụ vẽ địa hình.
- **Terrain Sample Asset Pack** (Unity) – terrain mẫu miễn phí.
- **Nature Starter Kit 2** (Unity) – cây, đá, cỏ miễn phí.
- **Particle Pack** (Unity) – bụi, khói, lửa, confetti miễn phí.
- **Quaternius** (CC0) – cây, đá, props low-poly.
- **Kenney Racing Pack** (CC0) – mô hình xe, đường đua, props.

### Xe & nhân vật
- **Mixamo** – nhân vật + animation miễn phí (cần tài khoản Adobe).
- **Unity Standard Assets** – xe mẫu, hiệu ứng bụi, âm thanh cũ nhưng miễn phí.
- **Sketchfab CC0/CC-BY** – mô hình xe off-road (ghi nguồn nếu CC-BY).

### Âm thanh
- **Free Sound Effects Pack** (Unity) – tiếng động cơ, va chạm, nitro.
- **Freesound.org** (CC0/CC-BY) – ghi nguồn nếu CC-BY.
- **Kenney Audio Pack** (CC0).

### Font
- **Be Vietnam Pro** và **Barlow Condensed** – giấy phép SIL Open Font License,
  đã đặt trong `Assets/09_HinhAnh/Resources/Fonts` kèm file license.

> Ghi chú: các gói trên chỉ là gợi ý. Đồ án hiện tại chạy độc lập, không cần tải thêm.

---

## 7. Kỹ thuật đáng chú ý

- **Địa hình procedural:** heightmap 513×513, carve mặt đường theo `RoadPath`,
  splat 4 lớp (nền, đất, đá, cỏ), normal map sinh bằng code.
- **Đường đua:** ribbon mesh + vai đường đất + vạch kẻ + vạch xuất phát/đích caro,
  `MeshCollider` để xe bám đường.
- **Xe arcade:** Rigidbody, lực kéo, lực bám ngang, drift, nitro, chống lật,
  raycast 4 bánh để bám địa hình, bụi bánh và vệt drift.
- **AI:** bám waypoint, nhìn trước theo tốc độ, giảm ga trước cua gắt, dùng nitro.
- **Giao diện:** toàn bộ sprite (rounded, ring, gauge, icon, gradient, noise)
  sinh bằng pixel trong `UISprites`; không cần ảnh UI.
- **Studio 3D:** tường cong, sàn phản chiếu, đèn spot/point, hạt bụi, vòng phát sáng.
- **Âm thanh:** synth bằng sóng sin/saw/noise, nhạc menu/race, tiếng động cơ thay đổi pitch theo tốc độ.
- **Tự kiểm thử:** `AutoTestRunner` chụp ảnh 6 màn, kiểm tra build tự động.

---

## 8. Ghi chú build

- Nếu Unity báo thiếu `UnityEngine.UI`, kiểm tra `Packages/manifest.json` có `"com.unity.ugui": "2.0.0"`.
- Nếu scene trống khi mở, chạy lại **DuaXe3D → 1. Thiết lập dự án (6 Scene)**.
- Layer `Car` được tạo tự động bởi `ProjectBuilder` (layer 8).
- Các shader `DuaXe3D/Mountain`, `DuaXe3D/PostFX` được thêm vào Always Included Shaders.
- Màu sắc dùng Linear Color Space để ánh sáng đẹp hơn.

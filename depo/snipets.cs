// ---------------------------------------------------------------------------- //

// Köşe tırmanmasını engelle
// PreventCornerClimb(ref moveDirection);

private void PreventCornerClimb(ref Vector3 desiredMoveDirection)
{
    _antiClimbRayHeightOffset = 0.05f;
    _antiClimbRayDistance = 0.4f; // 0.6f
    var controller = PlayerController.CharacterController;
    var position = controller.transform.position;
    var center = controller.center;
    var height = controller.height;
    var stepOffset = controller.stepOffset;
    
    // Işının başlangıç noktası: Karakterin merkezinin biraz altı, stepOffset'in hemen üzeri
    var rayOrigin = position + center - Vector3.up * 
        (height / 2f - (stepOffset + _antiClimbRayHeightOffset));

    // Sadece yatay harekete bakıyoruz
    var horizontalMoveDirection = new Vector3(desiredMoveDirection.x, 0, 
        desiredMoveDirection.z).normalized;

    RaycastHit hit;

    if (Physics.Raycast(rayOrigin, horizontalMoveDirection, out hit, 
            _antiClimbRayDistance, ClimbLayerMask))
    {
        #region ProjectOnPlane Slide
        #if false
        // ProjectOnPlane ile kaymasını sağlayabiliriz.
        var projectedMove = Vector3.ProjectOnPlane(desiredMoveDirection, hit.normal);
        if (Vector3.Dot(projectedMove, desiredMoveDirection) >= 0) // Geriye doğru itilmesini engelle
        {
            desiredMoveDirection = projectedMove.normalized * desiredMoveDirection.magnitude;
        }
        else // Çok dik bir açıysa, belki sadece o yöne hareketi kes
        {
            // Bu kısım daha karmaşık olabilir, en basit çözüm o yöne hareketi kısıtlamak
            // veya karakteri durdurmak olabilir.
            // Şimdilik engele doğru olan bileşeni azaltmayı deneyelim:
            var perpendicularToHit = Vector3.Cross(hit.normal, Vector3.up).normalized;
            var forwardComponent = Vector3.Dot(desiredMoveDirection, perpendicularToHit);
            desiredMoveDirection = perpendicularToHit * forwardComponent;
            Debug.Log($"Dik Aci : {desiredMoveDirection}");
            
            // Veya daha basiti, eğer çarptıysa o yönde ilerlemesini kısıtla
            // desiredMoveDirection = Vector3.zero; // Bu çok ani durdurur.
        }     
        #endif
        #endregion
        
        #region ProjectOnPlane Stop
        #if true
        // Basit bir engelleme için (kaydırmadan):
        // Eğer engel karakterin tam önündeyse ve dik bir yüzeyse, hareketi durdur.
        var angle = Vector3.Angle(Vector3.up, hit.normal);
        if (angle > 80 && angle < 100)
        {
           desiredMoveDirection = Vector3.ProjectOnPlane(desiredMoveDirection, hit.normal);

           IsMovementBlocked = desiredMoveDirection.sqrMagnitude <= _movementThreshold;
           // Debug.Log(IsMovementBlocked);
        }
        #endif
        #endregion
    }
}

// ---------------------------------------------------------------------------- //
   
private void DrawPreventCornerClimb()
{
    var controller = PlayerController.CharacterController;
    if (controller == null) return;
    
    // PreventCornerClimb methodundaki aynı parametreleri kullan
    _antiClimbRayHeightOffset = 0.05f;
    _antiClimbRayDistance = 0.4f; // 0.6f
    
    // Işının başlangıç noktası: PreventCornerClimb ile aynı
    var position = controller.transform.position;
    var center = controller.center;
    var height = controller.height;
    var stepOffset = controller.stepOffset;
    
    // Işının başlangıç noktası: Karakterin merkezinin biraz altı, stepOffset'in hemen üzeri
    var rayOrigin = position + center - Vector3.up * 
        (height / 2f - (stepOffset + _antiClimbRayHeightOffset));

    // Hareket yönünü almak için mevcut input'u kullan
    var moveDirection = new Vector3(AppliedMovementX, 0, AppliedMovementZ);
    
    // Eğer hareket yoksa, kameranın ileri yönünü kullan
    if (moveDirection.magnitude < 0.1f)
    {
        var cam = PlayerController.PlayerCamera.transform;
        var forward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
        moveDirection = forward;
    }
    else
    {
        // Kamera yönüne göre döndür (HandleMovement ile aynı)
        var cam = PlayerController.PlayerCamera.transform;
        var forward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
        var right = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;
        moveDirection = moveDirection.z * forward + moveDirection.x * right;
    }
    
    var horizontalMoveDirection = new Vector3(moveDirection.x, 0, moveDirection.z).normalized;
    
    // Raycast yap
    var hitDetected = Physics.Raycast(rayOrigin, horizontalMoveDirection, out var hit, 
        _antiClimbRayDistance, ClimbLayerMask);
    
    // Hit varsa kırmızı, yoksa sarı
    Gizmos.color = hitDetected ? Color.red : Color.yellow;
    
    // Gizmo çiz
    Gizmos.DrawWireSphere(rayOrigin, controller.radius * 0.5f);
    
    // Debug ray çiz
    if (hitDetected)
    {
        Gizmos.DrawLine(rayOrigin, hit.point);
        Gizmos.DrawSphere(hit.point, 0.05f);
    }
    else
    {
        Gizmos.DrawLine(rayOrigin, rayOrigin + horizontalMoveDirection * _antiClimbRayDistance);
    }
}

private void DrawStepOffsetGizmo()
{
    var controller = PlayerController.CharacterController;
    if (controller == null) return;

    // Character Controller'ın pozisyon ve boyut bilgileri
    var controllerTransform = controller.transform;
    var center = controllerTransform.position + controller.center;
    var radius = controller.radius;
    var stepOffset = controller.stepOffset;

    // Step offset yüksekliğindeki düzlemin konumu
    var stepPlaneY = controllerTransform.position.y + stepOffset;
    var stepPlaneCenter = new Vector3(center.x, stepPlaneY, center.z);

    // Gizmo rengi - step offset aktifken yeşil, değilse gri
    Gizmos.color = stepOffset > 0
        ? new Color(0f, 1f, 0f, 0.7f)
        : new Color(0.5f, 0.5f, 0.5f, 0.7f);

    // Düz plane (disk) şeklinde gizmo çiz
    // Unity'de düz disk çizmek için birden fazla çizgi kullanıyoruz
    var segments = 32;
    var angleStep = 360f / segments;

    for (var i = 0; i < segments; i++)
    {
        var angle1 = i * angleStep * Mathf.Deg2Rad;
        var angle2 = (i + 1) * angleStep * Mathf.Deg2Rad;

        var point1 = stepPlaneCenter + new Vector3(
            Mathf.Cos(angle1) * radius,
            0,
            Mathf.Sin(angle1) * radius
        );

        var point2 = stepPlaneCenter + new Vector3(
            Mathf.Cos(angle2) * radius,
            0,
            Mathf.Sin(angle2) * radius
        );

        // Dış çember
        Gizmos.DrawLine(point1, point2);

        // Merkezden dışa çizgiler (opsiyonel - daha belirgin görünüm için)
        if (i % 4 == 0) // Her 4. çizgide merkeze bağla
        {
            Gizmos.DrawLine(stepPlaneCenter, point1);
        }
    }

    // Merkez noktası
    Gizmos.color = new Color(1f, 0.5f, 0f, 1f); // Turuncu
    Gizmos.DrawWireSphere(stepPlaneCenter, 0.05f);

    // Step offset yüksekliğini gösteren dikey çizgi
    Gizmos.color = new Color(1f, 1f, 0f, 0.8f); // Sarı
    var groundLevel =
        new Vector3(center.x, controllerTransform.position.y, center.z);
    Gizmos.DrawLine(groundLevel, stepPlaneCenter);

    // Zemin seviyesi referans çizgisi
    Gizmos.color = new Color(0.3f, 0.3f, 0.3f, 0.6f); // Koyu gri
    for (var i = 0; i < 8; i++)
    {
        var angle = i * 45f * Mathf.Deg2Rad;
        var point = groundLevel + new Vector3(
            Mathf.Cos(angle) * radius * 0.8f,
            0,
            Mathf.Sin(angle) * radius * 0.8f
        );
        Gizmos.DrawLine(groundLevel, point);
    }
}

// ---------------------------------------------------------------------------- //

private bool CheckEdgeSupport(CharacterController controller, LayerMask layerMask)
{
    var radius = controller.radius;
    var supportRadius = radius * (1f - EdgeDetectionThreshold); // Daha küçük yarıçap
    var origin = controller.transform.position + controller.center - 
                 Vector3.up * (controller.height / 2f);
    
    // Merkez noktadan daha küçük yarıçapla kontrol et
    var centerSupported = Physics.CheckSphere(origin, 
        supportRadius * 0.5f, layerMask);
    
    if (!centerSupported)
    {
        _isNearEdge = true;
        return false; // Merkez desteklenmiyorsa düş
    }
    
    // 8 yönde edge detection
    var supportedDirections = 0;
    var totalDirections = 8;
    
    for (var i = 0; i < totalDirections; i++)
    {
        var angle = (360f / totalDirections) * i * Mathf.Deg2Rad;
        var direction = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
        var checkPoint = origin + direction * supportRadius;
        
        if (Physics.Raycast(checkPoint, Vector3.down,
                radius * 0.5f, layerMask))
        {
            supportedDirections++;
        }
    }
    
    // En az %60'ı desteklenmeli (5/8 yön)
    var supportPercentage = (float)supportedDirections / totalDirections;
    _isNearEdge = supportPercentage < 0.6f;
    
    return supportPercentage >= 0.6f;
}

// ---------------------------------------------------------------------------- //

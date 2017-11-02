N=100;
[X,Y] = meshgrid(1:N);
light = [50 90 5];
camera = [50 10 5];
normal = [0 0 1];
A = zeros(N,N);
for i = 1:N
    for j = 1:N
        toCamera = camera - [i j 0];
        toLight = light - [i j 0];
        toCamera = toCamera / norm(toCamera);
        toLight = toLight / norm(toLight);
        halfAngle = 0.5 * (toCamera + toLight);
        halfAngle = halfAngle / norm(halfAngle);
        A(j,i) = dot(halfAngle, normal);
    end;
end;
% A = X;
surf(X,Y,A)
xlabel('X-Axis')
ylabel('Y-Axis')
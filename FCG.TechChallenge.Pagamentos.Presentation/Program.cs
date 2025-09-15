using FCG.TechChallenge.Pagamentos.Application.DTOs;
using FCG.TechChallenge.Pagamentos.Application.Interfaces;
using FCG.TechChallenge.Pagamentos.Application.Service;
using FCG.TechChallenge.Pagamentos.Application.Validator;
using FCG.TechChallenge.Pagamentos.Infrastructure.Data;
using FCG.TechChallenge.Pagamentos.Infrastructure.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(opt =>
    opt.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)
    )
);

builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

builder.Services.AddValidatorsFromAssembly(typeof(CreatePaymentValidator).Assembly);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseExceptionHandler(a => a.Run(async ctx =>
{
    ctx.Response.StatusCode = 500;
    await ctx.Response.WriteAsJsonAsync(new { error = "Unexpected error" });
}));

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Group routes
var payments = app.MapGroup("/payments").WithTags("Payments");

// Create (Authorize)
payments.MapPost(
    "/",
    async Task<Results<Created<PaymentResponse>, ValidationProblem, ProblemHttpResult>>
    (CreatePaymentRequest req, IPaymentService service, IValidator<CreatePaymentRequest> validator, HttpContext http) =>
    {
        try
        {
            var v = await validator.ValidateAsync(req);
            if (!v.IsValid)
                return TypedResults.ValidationProblem(v.ToDictionary());

            var created = await service.AuthorizeAsync(req, http.RequestAborted);
            return TypedResults.Created($"/payments/{created.Id}", created);
        }
        catch (Exception ex)
        {
            return TypedResults.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Unexpected error while processing the payment"
            );
        }
    }
);

// Get by id
payments.MapGet(
    "/{id:int}",
    async Task<Results<Ok<PaymentResponse>, NotFound>>
    (int id, IPaymentService service, HttpContext http) =>
    {
        var res = await service.GetAsync(id, http.RequestAborted);
        return res is not null ? TypedResults.Ok(res) : TypedResults.NotFound();
    });

// Capture
payments.MapPost(
    "/{id:int}/capture",
    async Task<Results<Ok<PaymentResponse>, NotFound, BadRequest<string>>>
    (int id, IPaymentService service, HttpContext http) =>
    {
        try
        {
            var res = await service.CaptureAsync(id, http.RequestAborted);
            return TypedResults.Ok(res);
        }
        catch (KeyNotFoundException) { return TypedResults.NotFound(); }
        catch (InvalidOperationException e) { return TypedResults.BadRequest(e.Message); }
    });

// Refund
payments.MapPost(
    "/{id:int}/refund",
    async Task<Results<Ok<PaymentResponse>, NotFound, BadRequest<string>>>
    (int id, IPaymentService service, HttpContext http) =>
    {
        try
        {
            var res = await service.RefundAsync(id, http.RequestAborted);
            return TypedResults.Ok(res);
        }
        catch (KeyNotFoundException) { return TypedResults.NotFound(); }
        catch (InvalidOperationException e) { return TypedResults.BadRequest(e.Message); }
    }
);

app.MapHealthChecks("/health");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

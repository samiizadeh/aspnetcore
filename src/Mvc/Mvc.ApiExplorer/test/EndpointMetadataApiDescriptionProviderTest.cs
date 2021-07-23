// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    public class EndpointMetadataApiDescriptionProviderTest
    {
        [Fact]
        public void MultipleApiDescriptionsCreatedForMultipleHttpMethods()
        {
            var apiDescriptions = GetApiDescriptions(() => { }, "/", new string[] { "FOO", "BAR" });

            Assert.Equal(2, apiDescriptions.Count);
        }

        [Fact]
        public void ApiDescriptionNotCreatedIfNoHttpMethods()
        {
            var apiDescriptions = GetApiDescriptions(() => { }, "/", Array.Empty<string>());

            Assert.Empty(apiDescriptions);
        }

        [Fact]
        public void UsesDeclaringTypeAsControllerName()
        {
            var apiDescription = GetApiDescription(TestAction);

            var declaringTypeName = typeof(EndpointMetadataApiDescriptionProviderTest).Name;
            Assert.Equal(declaringTypeName, apiDescription.ActionDescriptor.RouteValues["controller"]);
        }

        [Fact]
        public void UsesApplicationNameAsControllerNameIfNoDeclaringType()
        {
            var apiDescription = GetApiDescription(() => { });

            Assert.Equal(nameof(EndpointMetadataApiDescriptionProviderTest), apiDescription.ActionDescriptor.RouteValues["controller"]);
        }

        [Fact]
        public void AddsJsonRequestFormatWhenFromBodyInferred()
        {
            static void AssertJsonRequestFormat(ApiDescription apiDescription)
            {
                var requestFormat = Assert.Single(apiDescription.SupportedRequestFormats);
                Assert.Equal("application/json", requestFormat.MediaType);
                Assert.Null(requestFormat.Formatter);
            }

            AssertJsonRequestFormat(GetApiDescription(
                (InferredJsonClass fromBody) => { }));

            AssertJsonRequestFormat(GetApiDescription(
                ([FromBody] int fromBody) => { }));
        }

        [Fact]
        public void AddsRequestFormatFromMetadata()
        {
            static void AssertustomRequestFormat(ApiDescription apiDescription)
            {
                var requestFormat = Assert.Single(apiDescription.SupportedRequestFormats);
                Assert.Equal("application/custom", requestFormat.MediaType);
                Assert.Null(requestFormat.Formatter);
            }

            AssertustomRequestFormat(GetApiDescription(
                [Consumes("application/custom")]
                (InferredJsonClass fromBody) => { }));

            AssertustomRequestFormat(GetApiDescription(
                [Consumes("application/custom")]
                ([FromBody] int fromBody) => { }));
        }

        [Fact]
        public void AddsMultipleRequestFormatsFromMetadata()
        {
            var apiDescription = GetApiDescription(
                [Consumes("application/custom0", "application/custom1")]
                (InferredJsonClass fromBody) => { });

            Assert.Equal(2, apiDescription.SupportedRequestFormats.Count);

            var requestFormat0 = apiDescription.SupportedRequestFormats[0];
            Assert.Equal("application/custom0", requestFormat0.MediaType);
            Assert.Null(requestFormat0.Formatter);

            var requestFormat1 = apiDescription.SupportedRequestFormats[1];
            Assert.Equal("application/custom1", requestFormat1.MediaType);
            Assert.Null(requestFormat1.Formatter);
        }

        [Fact]
        public void AddsJsonResponseFormatWhenFromBodyInferred()
        {
            static void AssertJsonResponse(ApiDescription apiDescription, Type expectedType)
            {
                var responseType = Assert.Single(apiDescription.SupportedResponseTypes);
                Assert.Equal(200, responseType.StatusCode);
                Assert.Equal(expectedType, responseType.Type);
                Assert.Equal(expectedType, responseType.ModelMetadata.ModelType);

                var responseFormat = Assert.Single(responseType.ApiResponseFormats);
                Assert.Equal("application/json", responseFormat.MediaType);
                Assert.Null(responseFormat.Formatter);
            }

            AssertJsonResponse(GetApiDescription(() => new InferredJsonClass()), typeof(InferredJsonClass));
            AssertJsonResponse(GetApiDescription(() => (IInferredJsonInterface)null), typeof(IInferredJsonInterface));
        }

        [Fact]
        public void AddsTextResponseFormatWhenFromBodyInferred()
        {
            var apiDescription = GetApiDescription(() => "foo");

            var responseType = Assert.Single(apiDescription.SupportedResponseTypes);
            Assert.Equal(200, responseType.StatusCode);
            Assert.Equal(typeof(string), responseType.Type);
            Assert.Equal(typeof(string), responseType.ModelMetadata.ModelType);

            var responseFormat = Assert.Single(responseType.ApiResponseFormats);
            Assert.Equal("text/plain", responseFormat.MediaType);
            Assert.Null(responseFormat.Formatter);
        }

        [Fact]
        public void AddsNoResponseFormatWhenItCannotBeInferredAndTheresNoMetadata()
        {
            static void AssertVoid(ApiDescription apiDescription)
            {
                var responseType = Assert.Single(apiDescription.SupportedResponseTypes);
                Assert.Equal(200, responseType.StatusCode);
                Assert.Equal(typeof(void), responseType.Type);
                Assert.Equal(typeof(void), responseType.ModelMetadata.ModelType);

                Assert.Empty(responseType.ApiResponseFormats);
            }

            AssertVoid(GetApiDescription(() => { }));
            AssertVoid(GetApiDescription(() => Task.CompletedTask));
            AssertVoid(GetApiDescription(() => new ValueTask()));
        }

        [Fact]
        public void AddsResponseFormatFromMetadata()
        {
            var apiDescription = GetApiDescription(
                [ProducesResponseType(typeof(TimeSpan), StatusCodes.Status201Created)]
                [Produces("application/custom")]
                () => new InferredJsonClass());

            var responseType = Assert.Single(apiDescription.SupportedResponseTypes);

            Assert.Equal(201, responseType.StatusCode);
            Assert.Equal(typeof(TimeSpan), responseType.Type);
            Assert.Equal(typeof(TimeSpan), responseType.ModelMetadata.ModelType);

            var responseFormat = Assert.Single(responseType.ApiResponseFormats);
            Assert.Equal("application/custom", responseFormat.MediaType);
        }

        [Fact]
        public void AddsMultipleResponseFormatsFromMetadataWithPoco()
        {
            var apiDescription = GetApiDescription(
                [ProducesResponseType(typeof(TimeSpan), StatusCodes.Status201Created)]
                [ProducesResponseType(StatusCodes.Status400BadRequest)]
                () => new InferredJsonClass());

            Assert.Equal(2, apiDescription.SupportedResponseTypes.Count);

            var createdResponseType = apiDescription.SupportedResponseTypes[0];

            Assert.Equal(201, createdResponseType.StatusCode);
            Assert.Equal(typeof(TimeSpan), createdResponseType.Type);
            Assert.Equal(typeof(TimeSpan), createdResponseType.ModelMetadata.ModelType);

            var createdResponseFormat = Assert.Single(createdResponseType.ApiResponseFormats);
            Assert.Equal("application/json", createdResponseFormat.MediaType);

            var badRequestResponseType = apiDescription.SupportedResponseTypes[1];

            Assert.Equal(400, badRequestResponseType.StatusCode);
            Assert.Equal(typeof(InferredJsonClass), badRequestResponseType.Type);
            Assert.Equal(typeof(InferredJsonClass), badRequestResponseType.ModelMetadata.ModelType);

            var badRequestResponseFormat = Assert.Single(badRequestResponseType.ApiResponseFormats);
            Assert.Equal("application/json", badRequestResponseFormat.MediaType);
        }

        [Fact]
        public void AddsMultipleResponseFormatsFromMetadataWithIResult()
        {
            var apiDescription = GetApiDescription(
                [ProducesResponseType(typeof(InferredJsonClass), StatusCodes.Status201Created)]
                [ProducesResponseType(StatusCodes.Status400BadRequest)]
                () => Results.Ok(new InferredJsonClass()));

            Assert.Equal(2, apiDescription.SupportedResponseTypes.Count);

            var createdResponseType = apiDescription.SupportedResponseTypes[0];

            Assert.Equal(201, createdResponseType.StatusCode);
            Assert.Equal(typeof(InferredJsonClass), createdResponseType.Type);
            Assert.Equal(typeof(InferredJsonClass), createdResponseType.ModelMetadata.ModelType);

            var createdResponseFormat = Assert.Single(createdResponseType.ApiResponseFormats);
            Assert.Equal("application/json", createdResponseFormat.MediaType);

            var badRequestResponseType = apiDescription.SupportedResponseTypes[1];

            Assert.Equal(400, badRequestResponseType.StatusCode);
            Assert.Equal(typeof(void), badRequestResponseType.Type);
            Assert.Equal(typeof(void), badRequestResponseType.ModelMetadata.ModelType);

            Assert.Empty(badRequestResponseType.ApiResponseFormats);
        }

        [Fact]
        public void AddsFromRouteParameterAsPath()
        {
            static void AssertPathParameter(ApiDescription apiDescription)
            {
                var param = Assert.Single(apiDescription.ParameterDescriptions);
                Assert.Equal(typeof(int), param.Type);
                Assert.Equal(typeof(int), param.ModelMetadata.ModelType);
                Assert.Equal(BindingSource.Path, param.Source);
            }

            AssertPathParameter(GetApiDescription((int foo) => { }, "/{foo}"));
            AssertPathParameter(GetApiDescription(([FromRoute] int foo) => { }));
        }

        [Fact]
        public void AddsFromQueryParameterAsQuery()
        {
            static void AssertQueryParameter(ApiDescription apiDescription)
            {
                var param = Assert.Single(apiDescription.ParameterDescriptions);
                Assert.Equal(typeof(int), param.Type);
                Assert.Equal(typeof(int), param.ModelMetadata.ModelType);
                Assert.Equal(BindingSource.Query, param.Source);
            }

            AssertQueryParameter(GetApiDescription((int foo) => { }, "/"));
            AssertQueryParameter(GetApiDescription(([FromQuery] int foo) => { }));
        }

        [Fact]
        public void AddsFromHeaderParameterAsHeader()
        {
            var apiDescription = GetApiDescription(([FromHeader] int foo) => { });
            var param = Assert.Single(apiDescription.ParameterDescriptions);

            Assert.Equal(typeof(int), param.Type);
            Assert.Equal(typeof(int), param.ModelMetadata.ModelType);
            Assert.Equal(BindingSource.Header, param.Source);
        }

        [Fact]
        public void DoesNotAddFromServiceParameterAsService()
        {
            Assert.Empty(GetApiDescription((IInferredServiceInterface foo) => { }).ParameterDescriptions);
            Assert.Empty(GetApiDescription(([FromServices] int foo) => { }).ParameterDescriptions);
            Assert.Empty(GetApiDescription((HttpContext context) => { }).ParameterDescriptions);
            Assert.Empty(GetApiDescription((HttpRequest request) => { }).ParameterDescriptions);
            Assert.Empty(GetApiDescription((HttpResponse response) => { }).ParameterDescriptions);
            Assert.Empty(GetApiDescription((ClaimsPrincipal user) => { }).ParameterDescriptions);
            Assert.Empty(GetApiDescription((CancellationToken token) => { }).ParameterDescriptions);
        }

        [Fact]
        public void AddsFromBodyParameterAsBody()
        {
            static void AssertBodyParameter(ApiDescription apiDescription, Type expectedType)
            {
                var param = Assert.Single(apiDescription.ParameterDescriptions);
                Assert.Equal(expectedType, param.Type);
                Assert.Equal(expectedType, param.ModelMetadata.ModelType);
                Assert.Equal(BindingSource.Body, param.Source);
            }

            AssertBodyParameter(GetApiDescription((InferredJsonClass foo) => { }), typeof(InferredJsonClass));
            AssertBodyParameter(GetApiDescription(([FromBody] int foo) => { }), typeof(int));
        }

        [Fact]
        public void AddsDefaultValueFromParameters()
        {
            var apiDescription = GetApiDescription(TestActionWithDefaultValue);

            var param = Assert.Single(apiDescription.ParameterDescriptions);
            Assert.Equal(42, param.DefaultValue);
        }

        [Fact]
        public void AddsMultipleParameters()
        {
            var apiDescription = GetApiDescription(([FromRoute] int foo, int bar, InferredJsonClass fromBody) => { });
            Assert.Equal(3, apiDescription.ParameterDescriptions.Count);

            var fooParam = apiDescription.ParameterDescriptions[0];
            Assert.Equal(typeof(int), fooParam.Type);
            Assert.Equal(typeof(int), fooParam.ModelMetadata.ModelType);
            Assert.Equal(BindingSource.Path, fooParam.Source);

            var barParam = apiDescription.ParameterDescriptions[1];
            Assert.Equal(typeof(int), barParam.Type);
            Assert.Equal(typeof(int), barParam.ModelMetadata.ModelType);
            Assert.Equal(BindingSource.Query, barParam.Source);

            var fromBodyParam = apiDescription.ParameterDescriptions[2];
            Assert.Equal(typeof(InferredJsonClass), fromBodyParam.Type);
            Assert.Equal(typeof(InferredJsonClass), fromBodyParam.ModelMetadata.ModelType);
            Assert.Equal(BindingSource.Body, fromBodyParam.Source);
        }

        [Fact]
        public void AddsDisplayNameFromRouteEndpoint()
        {
            var apiDescription = GetApiDescription(() => "foo", displayName: "FOO");

            Assert.Equal("FOO", apiDescription.ActionDescriptor.DisplayName);
        }

        [Fact]
        public void AddsMetadataFromRouteEndpoint()
        {
            var apiDescription = GetApiDescription([ApiExplorerSettings(IgnoreApi = true)]() => { });

            Assert.NotEmpty(apiDescription.ActionDescriptor.EndpointMetadata);

            var apiExplorerSettings = apiDescription.ActionDescriptor.EndpointMetadata
                .OfType<ApiExplorerSettingsAttribute>()
                .FirstOrDefault();

            Assert.NotNull(apiExplorerSettings);
            Assert.True(apiExplorerSettings.IgnoreApi);
        }

        private IList<ApiDescription> GetApiDescriptions(
            Delegate action,
            string pattern = null,
            IEnumerable<string> httpMethods = null,
            string displayName = null)
        {
            var methodInfo = action.Method;
            var attributes = methodInfo.GetCustomAttributes();
            var context = new ApiDescriptionProviderContext(Array.Empty<ActionDescriptor>());

            var httpMethodMetadata = new HttpMethodMetadata(httpMethods ?? new[] { "GET" });
            var metadataItems = new List<object>(attributes) { methodInfo, httpMethodMetadata };
            var endpointMetadata = new EndpointMetadataCollection(metadataItems.ToArray());
            var routePattern = RoutePatternFactory.Parse(pattern ?? "/");

            var endpoint = new RouteEndpoint(httpContext => Task.CompletedTask, routePattern, 0, endpointMetadata, displayName);
            var endpointDataSource = new DefaultEndpointDataSource(endpoint);
            var hostEnvironment = new HostEnvironment
            {
                ApplicationName = nameof(EndpointMetadataApiDescriptionProviderTest)
            };

            var provider = new EndpointMetadataApiDescriptionProvider(endpointDataSource, hostEnvironment, new ServiceProviderIsService());

            provider.OnProvidersExecuting(context);
            provider.OnProvidersExecuted(context);

            return context.Results;
        }

        private ApiDescription GetApiDescription(Delegate action, string pattern = null, string displayName = null) =>
            Assert.Single(GetApiDescriptions(action, pattern, displayName: displayName));

        private static void TestAction()
        {
        }

        private static void TestActionWithDefaultValue(int foo = 42)
        {
        }

        private class InferredJsonClass
        {
        }

        private interface IInferredServiceInterface
        {
        }

        private interface IInferredJsonInterface
        {
        }

        private class ServiceProviderIsService : IServiceProviderIsService
        {
            public bool IsService(Type serviceType) => serviceType == typeof(IInferredServiceInterface);
        }

        private class HostEnvironment : IHostEnvironment
        {
            public string EnvironmentName { get; set; }
            public string ApplicationName { get; set; }
            public string ContentRootPath { get; set; }
            public IFileProvider ContentRootFileProvider { get; set; }
        }
    }
}

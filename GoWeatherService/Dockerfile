FROM golang:1.22-alpine AS builder
WORKDIR /app
COPY go.mod ./
RUN go mod download
COPY *.go ./
RUN go build -o go-weather-service .

FROM alpine:3.19
WORKDIR /app
COPY --from=builder /app/go-weather-service .
EXPOSE 8090
CMD ["./go-weather-service"]

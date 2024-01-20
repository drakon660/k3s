client {
  enabled = true
}
server {
  enabled = true
  bootstrap_expect = 1
}
datacenter = "dc1"
data_dir = "/opt/nomad"
name =  "drakon"